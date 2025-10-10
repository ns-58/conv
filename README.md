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
