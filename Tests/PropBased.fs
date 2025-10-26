open OpenCvSharp
open FsCheck.FSharp
open Conv

let (@@) = (<|)

// let print_byte_matrix m =
//     let indr = ind m
//     for y in 0 .. m.Height - 1 do
//         for x in 0 .. m.Width do
//             printf "%d " @@ indr.get_Item(y,x)
//         printfn ""
//     printfn ""


let eq_to_opencv (m: Mat) (kernel: Mat) mode =
    use dest1 = new Mat(m.Height, m.Width, m.Type())
    use dest2 = new Mat(m.Height, m.Width, m.Type())

    conv m dest1 kernel mode
    Cv2.Filter2D(m, dest2, -1, kernel, borderType = BorderTypes.Replicate)

    let indr1, indr2 = ind dest1, ind dest2
    // print_byte_matrix dest1
    // print_byte_matrix dest2

    Seq.fold (&&) true
    @@ seq {
        for y in 0 .. dest1.Height - 1 do
            for x in 0 .. dest1.Width - 1 ->
                (int @@ indr1.get_Item (y, x)) >= (int @@ indr2.get_Item (y, x)) - 1
                && (int @@ indr1.get_Item (y, x)) <= (int @@ indr2.get_Item (y, x)) + 1
    }


let byte_mat_gen =
    Gen.sized (fun s ->
        let [| rows; colomns |] = Gen.sampleWithSize s 2 @@ Gen.choose (10, s + 1)

        Gen.map (fun (data: byte[,]) -> Mat.FromArray<byte>(data))
        @@ Gen.array2DOfDim rows colomns
        @@ Gen.map byte
        @@ Gen.choose (0, 255))

let gen_float =
    Gen.map (fun (m, n) -> float32 m / float32 n)
    @@ Gen.two
    @@ Gen.choose (System.Int32.MinValue / 3, System.Int32.MaxValue)


let float_mat_gen =
    Gen.sized (fun s ->
        let s = int @@ sqrt @@ float s
        let [| rows; colomns |] = Gen.sampleWithSize s 2 @@ Gen.choose (1, s + 1)

        Gen.map (fun (data: float32[,]) -> Mat.FromArray<float32>(data))
        @@ Gen.array2DOfDim rows colomns gen_float)

type MyGenerators =
    static member ByteMap() =
        { new FsCheck.Arbitrary<Mat<byte>>() with
            override _.Generator = byte_mat_gen
            override _.Shrinker _ = Seq.empty }

    static member FloatMap() =
        { new FsCheck.Arbitrary<Mat<float32>>() with
            override _.Generator = float_mat_gen
            override _.Shrinker _ = Seq.empty }

for mode in [ Seql; ByPixel; ByRow; ByColumn; ByRect(UnLimited, UnLimited) ] do
    FsCheck.Check.One(
        FsCheck.Config.Quick.WithArbitrary([ typeof<MyGenerators> ]),
        fun (m: Mat<byte>) (k: Mat<float32>) -> eq_to_opencv m k mode
    )

FsCheck.Check.One(
    FsCheck.Config.Quick.WithArbitrary([ typeof<MyGenerators> ]),
    fun (m: Mat<byte>) (k: Mat<float32>) (xS: uint16) ->
        eq_to_opencv m k @@ ByRect(Limited @@ max 1 @@ int xS, UnLimited)
)

FsCheck.Check.One(
    FsCheck.Config.Quick.WithArbitrary([ typeof<MyGenerators> ]),
    fun (m: Mat<byte>) (k: Mat<float32>) (xS: uint16) ->
        eq_to_opencv m k @@ ByRect(Limited @@ max 1 @@ int xS, UnLimited)
)

FsCheck.Check.One(
    FsCheck.Config.Quick.WithArbitrary([ typeof<MyGenerators> ]),
    fun (m: Mat<byte>) (k: Mat<float32>) (xS: uint16) (yS: uint16) ->
        eq_to_opencv m k
        @@ ByRect(Limited @@ max 1 @@ int xS, Limited @@ max 1 @@ int yS)
)
