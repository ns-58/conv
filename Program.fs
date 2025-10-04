open System.IO
open OpenCvSharp
open Conv
open System.Threading.Tasks.Dataflow

let (>>) f a = (fun () -> a) f

open System

type cnfg =
    { mutable srcdir: string
      mutable outdir: string
      mutable prefix: string
      mutable max_readers: int option
      mutable max_writers: int option
      mutable max_conv_performers: int option
      mutable max_queue_size: int option
      mutable conv_mode: paral_method }

let full_cycle_paral_conv cnfg kernel =

    let load name =
        let src = new Mat(name, ImreadModes.Grayscale)
        (src, name)

    let applyFilter (m: Mat, name) =
        let dest = new Mat(m.Height, m.Width, m.Type())
        conv m dest kernel cnfg.conv_mode
        m.Dispose()
        (dest, name)

    let save (m: Mat, name: string) =
        Cv2.ImWrite(sprintf "./%s/%s-%s" cnfg.outdir cnfg.prefix @@ Path.GetFileName name, m)
        |> ignore

        m.Dispose()

    let opts bounded_queue p_deg =
        ExecutionDataflowBlockOptions(
            MaxDegreeOfParallelism = Option.defaultValue DataflowBlockOptions.Unbounded p_deg,
            BoundedCapacity =
                match cnfg.max_queue_size with
                | Some n when bounded_queue -> n
                | _ -> DataflowBlockOptions.Unbounded
        )

    let link_opts = DataflowLinkOptions(PropagateCompletion = true)


    let loadBlock =
        TransformBlock<string, Mat * string>(
            load,
            opts true cnfg.max_readers

        )

    let processBlock =
        TransformBlock<Mat * string, Mat * string>(applyFilter, opts true cnfg.max_conv_performers)

    let saveBlock = ActionBlock<Mat * string>(save, opts true cnfg.max_writers)

    let waitingBlock = BufferBlock<string>()

    use _ = waitingBlock.LinkTo(loadBlock, link_opts)
    use _ = loadBlock.LinkTo(processBlock, link_opts)
    use _ = processBlock.LinkTo(saveBlock, link_opts)
    System.IO.Directory.CreateDirectory cnfg.outdir |> ignore

    Directory.GetFiles(cnfg.srcdir, "*.jpg", SearchOption.TopDirectoryOnly)
    |> Array.iter @@ fun name -> waitingBlock.Post name |> ignore

    waitingBlock.Complete()

    try
        saveBlock.Completion.Wait()
    with :? AggregateException as ex ->
        ex.InnerExceptions
        |> Seq.iter (fun e ->
            match e.Message with
            | "!_img.empty()" -> ()
            | _ -> Printf.eprintfn "Error: %s" e.Message)


let cnfg =
    { srcdir = "./"
      outdir = "./Out"
      prefix = "Conv"
      max_readers = Some 1
      max_writers = None
      max_conv_performers = Some 4
      max_queue_size = Some 4
      conv_mode = ByRow }

[<EntryPoint>]
let main _ =

    let kernel = new Mat([| 3; 3 |], MatType.CV_32FC1, Scalar -1.)
    ~~~ (ind_fl kernel).set_Item 1 1 @@ float32 8
    full_cycle_paral_conv cnfg kernel
    kernel.Dispose()
    0
