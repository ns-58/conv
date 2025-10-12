open System.IO
open OpenCvSharp
open Conv
open ReadConvWrite
open Argu
open System


type conv_mode =
    | Seql
    | ByPixel
    | ByRow
    | ByColumn
    | ByRect

type CliArgument =
    | [<AltCommandLine("-s")>] Source of pathes: string list
    | [<AltCommandLine("-r")>] Recursive
    | [<AltCommandLine("-k")>] Kernel of Kernels.kernel
    | [<AltCommandLine("-o")>] Out_Dir of path: string
    | [<AltCommandLine("-p")>] Prefix of str: string
    | [<AltCommandLine("-m")>] Conv_Mode of conv_mode
    | [<AltCommandLine("-rh")>] Rect_Height of num: int option
    | [<AltCommandLine("-rw")>] Rect_Width of num: int option
    | Max_Readers of num: int
    | Max_Conv_Workers of num: int
    | Max_Writers of num: int
    | Max_Queue_size of num: int

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Source _ -> "specify (list of) image/directory absolute pathes (Default: ./ )"
            | Kernel _ -> "specify kernel to be applied (Default:  id)"
            | Recursive -> "search in nested directories too"
            | Out_Dir _ -> "specify directory for output images (Default: ./Out )"
            | Prefix _ -> "specify prefix for output image names (Default: Conv)"
            | Conv_Mode _ -> "specify the rule for (inter-threading) data partition (Default: ByRow)"
            | Rect_Height _ -> "specify rectangle height for ByRect partition (Default: 9). -1 stands for image height"
            | Rect_Width _ -> "specify rectangle width for ByRect partition (Default: 9). -1 stands for image width"
            | Max_Readers _ ->
                "specify upper bound for image reading threads number (Default: 1). -1 stands for unbound"
            | Max_Conv_Workers _ ->
                "specify upper bound for image conv. threads number (Default: 4). -1 stands for unbound"
            | Max_Writers _ ->
                "specify upper bound for output image saving threads number (Default: -1). -1 stands for unbound"
            | Max_Queue_size _ -> "specify upper bound for images in queue (Default: 4). -1 stands for unbound"


[<EntryPoint>]
let main args =
    let parser =
        ArgumentParser.Create<CliArgument>(
            errorHandler =
                ProcessExiter(
                    colorizer =
                        function
                        | ErrorCode.HelpText -> None
                        | _ -> Some ConsoleColor.Red
                )
        )

    let args = parser.Parse args


    let int_hndl def =
        function
        | n when n >= 0 -> Some n
        | -1 -> None
        | _ -> Some def

    let rec_flag_hndl =
        function
        | true -> SearchOption.AllDirectories
        | false -> SearchOption.TopDirectoryOnly


    let mode_hndl def mode rect_width rect_height =
        match mode with
        | ByRect ->
            let size_hndl =
                function
                | Some -1 -> UnLimited
                | Some n when n >= 0 -> Limited n
                | _ -> Limited def

            paral_method.ByRect(size_hndl rect_width, size_hndl rect_height)

        | Seql -> paral_method.Seql
        | ByPixel -> paral_method.ByPixel
        | ByRow -> paral_method.ByRow
        | ByColumn -> paral_method.ByColumn


    let cnfg =
        { source = args.GetResult(Source, defaultValue = [ "./" ]), args.Contains Recursive |> rec_flag_hndl
          outdir = args.GetResult(Out_Dir, defaultValue = "./Out")
          prefix = args.GetResult(Prefix, defaultValue = "Conv")
          max_readers = int_hndl 1 @@ args.GetResult(Max_Readers, defaultValue = 1)
          max_conv_performers = int_hndl 4 @@ args.GetResult(Max_Conv_Workers, defaultValue = 4)
          max_writers = int_hndl (-1) @@ args.GetResult(Max_Writers, defaultValue = -1)
          max_queue_size = int_hndl 4 @@ args.GetResult(Max_Queue_size, defaultValue = 4)
          conv_mode =
            mode_hndl
                9
                (args.GetResult(Conv_Mode, defaultValue = ByRow))
                (args.GetResult(Rect_Width, defaultValue = None))
                (args.GetResult(Rect_Height, defaultValue = None)) }


    let kernel =
        Mat.FromArray(Kernels.to_2d_array @@ args.GetResult(Kernel, defaultValue = Kernels.Id))

    full_cycle_paral_conv cnfg kernel
    kernel.Dispose()
    0
