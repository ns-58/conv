open OpenCvSharp

let (@@) = (<|)
let (>>) f a = (fun () -> a) f

let curry3 f a1 a2 a3 = f (a1, a2, a3)
let (~~~) = curry3

let ind (m: Mat) = m.GetGenericIndexer<byte>()
let ind_fl (m: Mat) = m.GetGenericIndexer<float32>()

let conv src dest kernel =

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

    for y in 0 .. src.Height - 1 do
        for x in 0 .. src.Width - 1 do
            ~~~ dest_indr.set_Item y x @@ calc1 x y