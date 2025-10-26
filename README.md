# The tool for image convolution
## Usage:  
Clone repo:
  ```bash
  $ git clone https://github.com/ns-58/conv
  $ cd conv
  ```
Build app:
  ```bash
  $ dotnet publish -c release
  $ ln -s ./bin/release/net6.0/publish/conv conv
  ```
Use --help for app usage guide:
  ```bash
  $ ./conv --help
USAGE: conv [--help] [--source [<pathes>...]] [--recursive]
            [--kernel <id|box3x3|box5x5|box7x7|gaussian|dog|lapl8neigh|lapl4neigh|lapl5x5|lapl5x5log|sobeln|sobels|sobelw|sobele>]
            [--out-dir <path>] [--prefix <str>] [--conv-mode <seql|bypixel|byrow|bycolumn|byrect>]
            [--rect-height [<num>]] [--rect-width [<num>]] [--max-readers <num>] [--max-conv-workers <num>]
            [--max-writers <num>] [--max-queue-size <num>]

OPTIONS:

    --source, -s [<pathes>...]
                          specify (list of) image/directory pathes (Default: ./ )
    --recursive, -r       search in nested directories too
    --kernel, -k <id|box3x3|box5x5|box7x7|gaussian|dog|lapl8neigh|lapl4neigh|lapl5x5|lapl5x5log|sobeln|sobels|sobelw|sobele>
                          specify kernel to be applied (Default:  id)
    --out-dir, -o <path>  specify directory for output images (Default: ./Out )
    --prefix, -p <str>    specify prefix for output image names (Default: Conv)
    --conv-mode, -m <seql|bypixel|byrow|bycolumn|byrect>
                          specify the rule for (inter-threading) data partition (Default: ByRow)
    --rect-height, -rh [<num>]
                          specify rectangle height for ByRect partition (Default: 9). -1 stands for image
                          height
    --rect-width, -rw [<num>]
                          specify rectangle width for ByRect partition (Default: 9). -1 stands for image
                          width
    --max-readers <num>   specify upper bound for image reading threads number (Default: 1). -1 stands for
                          unbound
    --max-conv-workers <num>
                          specify upper bound for image conv. threads number (Default: 4). -1 stands for
                          unbound
    --max-writers <num>   specify upper bound for output image saving threads number (Default: -1). -1
                          stands for unbound
    --max-queue-size <num>
                          specify upper bound for images in queue (Default: 4). -1 stands for unbound
    --help                display this list of options.
  ```
## Additional requirements:
  opencv4.7+  
  consider use of container https://github.com/users/shimat/packages/container/package/opencvsharp%2Fubuntu22-dotnet6sdk-opencv4.7.0

## Benchmarking
BenchmarkDotNet v0.13.8, Ubuntu 22.04.1 LTS (Jammy Jellyfish) (container)
11th Gen Intel Core i7-11850H 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 6.0.405
  [Host]     : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT AVX2
### Image partition
sizes:
small.jpg --- 750x750    (108KB)  
  big.jpg --- 11256x4877 (59MB)

| Method              | file         | Mean       | Error    | StdDev   | Gen0         | Gen1       | Gen2      | Allocated  |
|-------------------- |------------- |-----------:|---------:|---------:|-------------:|------------:|----------:|-----------:|
| Seql                | ../small.jpg |   670.9 ms |  1.14 ms |  1.01 ms |   53000.0000 |           - |         - |  643.78 MB |
| ByPixel             | ../small.jpg |   360.1 ms |  3.03 ms |  2.83 ms |   54000.0000 |   1500.0000 |         - |  646.74 MB |
| ByRow               | ../small.jpg |   273.4 ms |  3.12 ms |  2.43 ms |   53500.0000 |   1000.0000 |         - |   643.8 MB |
| ByColumn            | ../small.jpg |   286.2 ms |  3.31 ms |  3.09 ms |   53500.0000 |   1000.0000 |         - |   643.8 MB |
| ByRect1x1           | ../small.jpg | 1,192.1 ms | 23.46 ms | 45.75 ms |  216000.0000 |  24000.0000 | 1000.0000 | 2580.61 MB |
| ByRect1ker          | ../small.jpg |   482.6 ms |  5.95 ms |  5.28 ms |   95000.0000 |  16000.0000 |         - | 1144.73 MB |
| ByRect5ker          | ../small.jpg |   324.7 ms |  6.28 ms |  6.45 ms |   61000.0000 |   2000.0000 |         - |  731.14 MB |
| ByRect10ker         | ../small.jpg |   338.2 ms |  8.71 ms | 25.69 ms |   57500.0000 |   1500.0000 |         - |  686.81 MB |
| ByRect15ker         | ../small.jpg |   300.8 ms |  3.49 ms |  3.27 ms |   56000.0000 |   1000.0000 |         - |   672.3 MB |
| By1KerHeightRows    | ../small.jpg |   378.0 ms |  4.79 ms |  4.00 ms |   71000.0000 |   1000.0000 |         - |  858.11 MB |
| By3KerHeightRows    | ../small.jpg |   314.2 ms |  6.08 ms |  5.39 ms |   59000.0000 |   1000.0000 |         - |  715.24 MB |
| By10KerHeightRows   | ../small.jpg |   297.4 ms |  5.77 ms |  6.17 ms |   55500.0000 |   1000.0000 |         - |  664.96 MB |
| By1KerWidthColomns  | ../small.jpg |   394.5 ms |  7.42 ms |  6.94 ms |   71000.0000 |   1000.0000 |         - |  858.09 MB |
| By3KerWidthColomns  | ../small.jpg |   310.6 ms |  4.66 ms |  4.13 ms |   59000.0000 |   1000.0000 |         - |  715.24 MB |
| By10KerWidthColomns | ../small.jpg |   296.1 ms |  5.83 ms |  6.48 ms |   55500.0000 |   1000.0000 |         - |  664.95 MB |
| -                   |-             |-           |-         |-         |-             |-            |-          |-           |
| ByRow               | ../big.jpg   |    39.11 s |  0.323 s |  0.302 s | 7229000.0000 | 147000.0000 |         - |   84.26 GB |
| ByColumn            | ../big.jpg   |    37.33 s |  0.166 s |  0.155 s | 7227000.0000 | 139000.0000 |         - |   84.26 GB |
| ByRect15ker         | ../big.jpg   |    67.81 s |  1.350 s |  3.556 s | 7564000.0000 | 272000.0000 | 9000.0000 |   88.04 GB |
| By10KerHeightRows   | ../big.jpg   |    38.16 s |  0.295 s |  0.246 s | 7471000.0000 | 218000.0000 | 1000.0000 |   87.07 GB |
| By10KerWidthColomns | ../big.jpg   |    37.44 s |  0.179 s |  0.150 s | 7468000.0000 | 142000.0000 |         - |   87.07 GB |


// * Legends *  
  file      : Value of the 'file' parameter  
  Mean      : Arithmetic mean of all measurements  
  Error     : Half of 99.9% confidence interval  
  StdDev    : Standard deviation of all measurements  
  Gen0      : GC Generation 0 collects per 1000 operations  
  Gen1      : GC Generation 1 collects per 1000 operations  
  Gen2      : GC Generation 2 collects per 1000 operations  
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)  

Итог: благодаря современным компиляторам/вычислителям способ разбиения изображения (по строчкам/столбцам/прямоугольникам) не оказывает существенного влияния на производительность свертки; на первую роль выходят размер разбиения и сложность опредения границ его участков
  
### Pipelining

input: 99 650x650 images

| Method          | Mean    | Error   | StdDev  | Median  |
|---------------- |--------:|--------:|--------:|--------:|
| R1C1parW1Qub    | 35.75 s | 0.563 s | 1.199 s | 35.55 s |
| R1C1parW1Q1     | 35.74 s | 0.240 s | 0.225 s | 35.77 s | 
| R1C4parW1Q4     | 48.96 s | 0.954 s | 1.240 s | 48.72 s | 
| R1C4parW1Q8     | 50.66 s | 0.750 s | 0.702 s | 50.55 s |
| R1C4parWubQ4    | 51.31 s | 1.019 s | 2.279 s | 52.50 s | 
| R1CubseqlWubQ16 | 27.83 s | 0.187 s | 0.175 s | 27.91 s |
| R1C2parWubQ2    | 40.61 s | 0.439 s | 0.411 s | 40.66 s | 

where Rn --- n readers, Cn --- n convolutions, Wn --- n writers, Qn --- queue with size n,  ub --- unbound  

Наблюдения и итоги:
  одно чтение и одна запись способны "обслуживать" несколько сверток;  
  параллелить (внутри) несколько одновременных сверток неэфективно (из-за синхронизации на двух уровнях?);  
  наиболее эффективны вариант с одной параллельной сверткой и вариант с большим кол-вом последовательных, он быстрее всех (синхронизация внутри свертки нужна чаще);  


