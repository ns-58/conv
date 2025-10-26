module Conv

open OpenCvSharp
open System.Threading.Tasks

let (@@) = (<|)

let curry3 f a1 a2 a3 = f (a1, a2, a3)
let (~~~) = curry3

let ind (m: Mat) = m.GetGenericIndexer<byte>()
let ind_fl (m: Mat) = m.GetGenericIndexer<float32>()

type rectSideSize =
    | UnLimited
    | Limited of int

type paral_method =
    | Seql
    | ByPixel
    | ByRow
    | ByColumn
    | ByRect of rectSideSize * rectSideSize

let conv src dest kernel paral_method =

    let src_indr, dest_indr, kernel_indr = ind src, ind dest, ind_fl kernel

    let calc1 x y =
        byte
        @@ min (float32 255)
        @@ max (float32 0)
        @@ Seq.fold (+) (float32 0)
        @@ seq {
            for ky in 0 .. kernel.Height - 1 do
                for kx in 0 .. kernel.Width - 1 ->
                    let m, n = (kernel.Height / 2) - ky, (kernel.Width / 2) - kx
                    let (-) x y upper = max 0 @@ min (upper - 1) @@ (-) x y

                    (float32 @@ src_indr.get_Item ((-) y m src.Height, (-) x n src.Width))
                    * (kernel_indr.get_Item (ky, kx))
        }

    match paral_method with
    | Seql ->
        for y in 0 .. src.Height - 1 do
            for x in 0 .. src.Width - 1 do
                ~~~ dest_indr.set_Item y x @@ calc1 x y
    | ByPixel ->
        for y in 0 .. src.Height - 1 do
            Parallel.For(
                0,
                src.Width,
                fun x ->

                    ~~~ dest_indr.set_Item y x @@ calc1 x y

            )
            |> ignore
    | ByRow ->
        Parallel.For(
            0,
            src.Height,
            fun y ->
                for x in 0 .. src.Width - 1 do
                    ~~~ dest_indr.set_Item y x @@ calc1 x y
        )
        |> ignore
    | ByColumn ->
        Parallel.For(
            0,
            src.Width,
            fun x ->
                for y in 0 .. src.Height - 1 do
                    ~~~ dest_indr.set_Item y x @@ calc1 x y
        )
        |> ignore
    | ByRect(xS, yS) ->

        let xS, yS =
            (fun f -> f src.Height xS, f src.Width yS)
            @@ fun def ->
                function
                | Limited n when n > 0 && n < def -> n
                | UnLimited -> def
                | Limited n when n > 0 -> def
                | _ -> failwith "side size shoudn't be less then 1 pixel"

        Parallel.ForEach(
            seq {
                for starty in 0..yS .. src.Height - 1 do
                    for startx in 0..xS .. src.Width - 1 -> (starty, startx)
            },
            fun (starty, startx) ->
                for y in starty .. min (src.Height - 1) (starty + yS) do
                    for x in startx .. min (src.Width - 1) (startx + xS) do
                        ~~~ dest_indr.set_Item y x @@ calc1 x y
        )
        |> ignore
