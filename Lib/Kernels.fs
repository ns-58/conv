module Kernels

let (@@) = (<|)

type kernel =
    | Id
    | Box3x3
    | Box5x5
    | Box7x7
    | Gaussian
    | DoG // Difference of Gaussians
    | Lapl8Neigh //Laplacian with 8 (eq-t) neighbours
    | Lapl4Neigh
    | Lapl5x5
    | Lapl5x5LoG //Laplacian Of Gaussians
    | SobelN // Directional: from North to South
    | SobelS
    | SobelW
    | SobelE

let to_2d_array: kernel -> float32[,] =

    let distance_based_3x3 z o t =
        Array2D.init 3 3
        @@ fun y x ->
            float32
            @@ let dx, dy = abs (1 - x), abs (1 - y) in

               match dx + dy with
               | 0 -> z
               | 1 -> o
               | 2 -> t

    let distance_based_5x5 z o t_eq t th f =
        Array2D.init 5 5
        @@ fun y x ->
            float32
            @@ let dx, dy = abs (2 - x), abs (2 - y) in

               match dx + dy with
               | 0 -> z
               | 1 -> o
               | 2 when dx = dy -> t_eq
               | 2 -> t
               | 3 -> th
               | 4 -> f

    function
    | Id ->
        let arr = Array2D.create 3 3 @@ float32 0
        arr[1, 1] <- float32 1
        arr
    | Box3x3 -> Array2D.create 3 3 @@ float32 (1. / 9.)
    | Box5x5 -> Array2D.create 5 5 @@ float32 (1. / 25.)
    | Box7x7 -> Array2D.create 7 7 @@ float32 (1. / 49.)
    | Gaussian -> distance_based_5x5 0.150342 0.094907 0.059912 0.023792 0.015019 0.003765
    | DoG -> distance_based_3x3 1. -0.155615 -0.0943852
    | Lapl8Neigh ->
        let arr = Array2D.create 3 3 @@ float32 -1
        arr[1, 1] <- float32 8
        arr
    | Lapl4Neigh -> distance_based_3x3 4. -1. 0.
    | Lapl5x5 -> distance_based_5x5 4. 3. 2. 0. -1. -4.
    | Lapl5x5LoG -> distance_based_5x5 16. -2. -1. -1. 0. 0.
    | SobelN ->
        Array2D.init 3 3
        @@ fun y x ->
            float32
            @@ match y, x with
               | 1, _ -> 0
               | 0, 1 -> 2
               | 2, 1 -> -2
               | 0, _ -> 1
               | 2, _ -> -1
    | SobelS ->
        Array2D.init 3 3
        @@ fun y x ->
            float32
            @@ match y, x with
               | 1, _ -> 0
               | 0, 1 -> -2
               | 2, 1 -> 2
               | 0, _ -> -1
               | 2, _ -> 1
    | SobelW ->
        Array2D.init 3 3
        @@ fun y x ->
            float32
            @@ match x, y with
               | 1, _ -> 0
               | 0, 1 -> 2
               | 2, 1 -> -2
               | 0, _ -> 1
               | 2, _ -> -1
    | SobelE ->
        Array2D.init 3 3
        @@ fun y x ->
            float32
            @@ match x, y with
               | 1, _ -> 0
               | 0, 1 -> -2
               | 2, 1 -> 2
               | 0, _ -> -1
               | 2, _ -> 1
