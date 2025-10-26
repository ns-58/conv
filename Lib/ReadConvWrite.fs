open System.IO
open OpenCvSharp
open Conv
open System.Threading.Tasks.Dataflow

type source = string list * SearchOption


type cnfg =
    { source: source
      outdir: string
      prefix: string
      max_readers: int option
      max_writers: int option
      max_conv_performers: int option
      max_queue_size: int option
      conv_mode: paral_method }

let (~~~) = curry3


let load name =
    let src = new Mat(name, ImreadModes.Grayscale)
    (src, name)

let applyFilter conv_mode kernel (m: Mat, name) =
    let dest = new Mat(m.Height, m.Width, m.Type())
    conv m dest kernel conv_mode
    m.Dispose()
    (dest, name)

let save outdir prefix (m: Mat, name: string) =
    Cv2.ImWrite(sprintf "./%s/%s-%s" outdir prefix @@ Path.GetFileName name, m)
    |> ignore

    m.Dispose()

let full_cycle_paral_conv cnfg kernel =

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
        TransformBlock<Mat * string, Mat * string>(
            applyFilter cnfg.conv_mode kernel,
            opts true cnfg.max_conv_performers
        )

    let saveBlock =
        ActionBlock<Mat * string>(save cnfg.outdir cnfg.prefix, opts true cnfg.max_writers)

    let waitingBlock = BufferBlock<string>()

    use _ = waitingBlock.LinkTo(loadBlock, link_opts)
    use _ = loadBlock.LinkTo(processBlock, link_opts)
    use _ = processBlock.LinkTo(saveBlock, link_opts)
    Directory.CreateDirectory cnfg.outdir |> ignore

    let pathes, opts = cnfg.source

    List.iter (fun name -> waitingBlock.Post name |> ignore)
    @@ List.fold
        (fun files path ->
            if Directory.Exists path then
                List.append (List.ofArray @@ Directory.GetFiles(path, "*.jpg", opts)) files
            elif File.Exists path then
                path :: files
            else
                eprintfn "file %s wasn't found!" path
                files)
        []
        pathes

    waitingBlock.Complete()


    saveBlock.Completion.Wait()
