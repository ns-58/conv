open Conv
open System.IO
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open OpenCvSharp
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open ReadConvWrite


let kernel = new Mat([| 3; 3 |], MatType.CV_32FC1, Scalar -1.)
~~~ (ind_fl kernel).set_Item 1 1 @@ float32 8


[<MemoryDiagnoser>]
type PartitionConvComparision() =

    [<Params("../small.jpg", "../big.jpg")>]
    member val file = "" with get, set

    member self.getPath([<CallerFilePath; Optional; DefaultParameterValue("")>] path: string) =
        Path.GetDirectoryName path

    member self.Src =
        let path = Path.Combine [| self.getPath (); self.file |]
        let src = new Mat(path, ImreadModes.Grayscale)
        src

    member self.Dest = new Mat(self.Src.Height, self.Src.Width, self.Src.Type())


    [<Benchmark>]
    member self.Seql() = conv self.Src self.Dest kernel Seql

    [<Benchmark>]
    member self.ByPixel() = conv self.Src self.Dest kernel ByPixel

    [<Benchmark>]
    member self.ByRow() = conv self.Src self.Dest kernel ByRow

    [<Benchmark>]
    member self.ByColumn() = conv self.Src self.Dest kernel ByColumn

    [<Benchmark>]
    member self.ByRect1x1() =
        conv self.Src self.Dest kernel @@ ByRect(Limited 1, Limited 1)

    [<Benchmark>]
    member self.ByRect1ker() =
        conv self.Src self.Dest kernel
        @@ ByRect(Limited kernel.Width, Limited kernel.Height)

    [<Benchmark>]
    member self.ByRect5ker() =
        conv self.Src self.Dest kernel
        @@ ByRect(Limited @@ ( * ) 5 kernel.Width, Limited @@ ( * ) 5 kernel.Height)

    [<Benchmark>]
    member self.ByRect10ker() =
        conv self.Src self.Dest kernel
        @@ ByRect(Limited @@ ( * ) 10 kernel.Width, Limited @@ ( * ) 10 kernel.Height)

    [<Benchmark>]
    member self.ByRect15ker() =
        conv self.Src self.Dest kernel
        @@ ByRect(Limited @@ ( * ) 15 kernel.Width, Limited @@ ( * ) 15 kernel.Height)


    [<Benchmark>]
    member self.By1KerHeightRows() =
        conv self.Src self.Dest kernel @@ ByRect(UnLimited, Limited kernel.Height)

    [<Benchmark>]
    member self.By3KerHeightRows() =
        conv self.Src self.Dest kernel
        @@ ByRect(UnLimited, Limited @@ ( * ) 3 kernel.Height)

    [<Benchmark>]
    member self.By10KerHeightRows() =
        conv self.Src self.Dest kernel
        @@ ByRect(UnLimited, Limited @@ ( * ) 10 kernel.Height)

    [<Benchmark>]
    member self.By1KerWidthColomns() =
        conv self.Src self.Dest kernel @@ ByRect(Limited kernel.Width, UnLimited)

    [<Benchmark>]
    member self.By3KerWidthColomns() =
        conv self.Src self.Dest kernel
        @@ ByRect(Limited @@ ( * ) 3 kernel.Width, UnLimited)

    [<Benchmark>]
    member self.By10KerWidthColomns() =
        conv self.Src self.Dest kernel
        @@ ByRect(Limited @@ ( * ) 10 kernel.Width, UnLimited)


let def_cnfg =
    { source = ([ "/root/conv/pics" ], SearchOption.TopDirectoryOnly)
      outdir = "./Out"
      prefix = "Conv"
      max_readers = Some 1
      max_writers = None
      max_conv_performers = Some 4
      max_queue_size = Some 4
      conv_mode = ByRow }


[<MemoryDiagnoser>]
type Pipelining() =

    [<Benchmark>]
    member self.R1C1parW1Qub() =
        full_cycle_paral_conv
            { def_cnfg with
                max_readers = Some 1
                max_writers = Some 1
                max_conv_performers = Some 1
                max_queue_size = None
                conv_mode = ByRow }
            kernel

    [<Benchmark>]
    member self.R1C1parW1Q1() =
        full_cycle_paral_conv
            { def_cnfg with
                max_readers = Some 1
                max_writers = Some 1
                max_conv_performers = Some 1
                max_queue_size = Some 1
                conv_mode = ByRow }
            kernel


    [<Benchmark>]
    member self.R1C4parW1Q4() =
        full_cycle_paral_conv
            { def_cnfg with
                max_readers = Some 1
                max_writers = Some 1
                max_conv_performers = Some 4
                max_queue_size = Some 4
                conv_mode = ByRow }
            kernel

    [<Benchmark>]
    member self.R1C4parW1Q8() =
        full_cycle_paral_conv
            { def_cnfg with
                max_readers = Some 1
                max_writers = Some 1
                max_conv_performers = Some 4
                max_queue_size = Some 8
                conv_mode = ByRow }
            kernel

    [<Benchmark>]
    member self.R1C4parWubQ4() =
        full_cycle_paral_conv
            { def_cnfg with
                max_readers = Some 1
                max_writers = None
                max_conv_performers = Some 4
                max_queue_size = Some 4
                conv_mode = ByRow }
            kernel

    [<Benchmark>]
    member self.R1CubseqlWubQ16() =
        full_cycle_paral_conv
            { def_cnfg with
                max_readers = Some 1
                max_writers = None
                max_conv_performers = None
                max_queue_size = Some 16
                conv_mode = Seql }
            kernel

    [<Benchmark>]
    member self.R1C2parWubQ2() =
        full_cycle_paral_conv
            { def_cnfg with
                max_readers = Some 1
                max_writers = None
                max_conv_performers = Some 2
                max_queue_size = Some 4
                conv_mode = ByRow }
            kernel
    
    [<IterationCleanupAttribute>]
    member self.Cleanup()  =
        Directory.Delete (def_cnfg.outdir, recursive = true )

[<EntryPoint>]
let main _ =
    BenchmarkRunner.Run typeof<Pipelining> |> ignore
    0
