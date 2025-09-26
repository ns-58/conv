open System.IO
open OpenCvSharp
open Conv

[<EntryPoint>]
let main =
    function
    | [| imagePath |] ->
        use src = new Mat(imagePath, ImreadModes.Grayscale)
        use dest = new Mat(src.Height, src.Width, src.Type())
        use kernel = new Mat([| 3; 3 |], MatType.CV_32FC1, Scalar 0.)
        ~~~ (ind_fl kernel).set_Item 1 1 @@ float32 1


        conv src dest kernel

        >> Cv2.ImWrite(sprintf "%s-%s" "id" @@ Path.GetFileName imagePath, dest)
        |> ignore

        0
    | _ -> failwith ""
