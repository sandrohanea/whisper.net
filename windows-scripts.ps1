


function BuildWindows() {
    param(
        [Parameter(Mandatory = $true)] [string]$Arch,
        [Parameter(Mandatory = $false)] [bool]$Cuda = $false,
        [Parameter(Mandatory = $false)] [bool]$Vulkan = $false,
        [Parameter(Mandatory = $false)] [bool]$OpenVino = $false,
        [Parameter(Mandatory = $false)] [bool]$NoAvx = $false,
        [Parameter(Mandatory = $false)] [string]$Configuration = "Release"
    )

    # Ensure the build directory exists
    if (!(Test-Path "build")) {
        New-Item -ItemType Directory -Force -Path "build"
    }

    Write-Host "Building Windows binaries for $Arch (using Clang + Ninja) with cuda: $Cuda"

    $buildDirectory = "build/win-$Arch"
    $options = @(
        "-S", ".", 
        "-G", "Ninja Multi-Config",
        "-DGGML_NATIVE=OFF",
        "-DCMAKE_SYSTEM_NAME=Windows"
    )
    
    $options += "-DCMAKE_TOOLCHAIN_FILE=cmake/$Arch-windows-llvm.cmake"

    $avxOptions = @("-DGGML_AVX=ON", "-DGGML_AVX2=ON", "-DGGML_FMA=ON", "-DGGML_F16C=ON")
    
    $runtimePath = "./runtimes/Whisper.net.Runtime"

    if ($Cuda) {
        $options += "-DGGML_CUDA=1"
        $buildDirectory += "-cuda"
        $runtimePath += ".Cuda.Windows"
    }

    if ($Vulkan) {
        $options += "-DGGML_VULKAN=1"
        $buildDirectory += "-vulkan"
        $runtimePath += ".Vulkan"
    }
    
    if ($OpenVino) {
        $options += "-DWHISPER_OPENVINO=1"
        $buildDirectory += "-openvino"
        $runtimePath += ".OpenVino"
    }

    if ($NoAvx) {
        $avxOptions = @("-DGGML_AVX=OFF", "-DGGML_AVX2=OFF", "-DGGML_FMA=OFF", "-DGGML_F16C=OFF")
        $buildDirectory += "-noavx"
        $runtimePath += ".NoAvx"
    }

    # Add AVX flags
    $options += $avxOptions

    # Specify the out-of-source build directory
    $options += "-B"
    $options += $buildDirectory

    # Clean up any old build directory if desired
    if (Test-Path $buildDirectory) {
        Write-Host "Deleting old build files for $buildDirectory"
        Remove-Item -Force -Recurse -Path $buildDirectory
    }

    # Ensure CMake is available. This part is optional if you already have cmake in your PATH.
    $cmakePath = (Get-Command cmake -ErrorAction SilentlyContinue).Source
    if ([string]::IsNullOrEmpty($cmakePath)) {
        # Attempt to locate CMake in Visual Studio
        $visualStudioPath = Get-VisualStudioCMakePath
        if ([string]::IsNullOrEmpty($visualStudioPath)) {
            Write-Host "CMake is not found in the system or Visual Studio."
            return
        }
        $env:Path += ";$visualStudioPath"
    }

    # Create fresh build directory
    New-Item -ItemType Directory -Force -Path $buildDirectory

    # Call CMake to generate the build files
    Write-Host "Running: cmake $($options -join ' ')"
    cmake $options

    # Build with the specified configuration
    Write-Host "Building with configuration $Configuration"
    cmake --build $buildDirectory --config $Configuration

    # Create final output directories
    if (-not(Test-Path $runtimePath)) {
        New-Item -ItemType Directory -Force -Path $runtimePath
    }
    $runtimePath += "/win-$Arch"
    if (-not(Test-Path $runtimePath)) {
        New-Item -ItemType Directory -Force -Path $runtimePath
    }

    # Copy the generated DLLs (assuming same folder structure/names)
    Move-Item "$buildDirectory/bin/Release/whisper.dll" "$runtimePath/whisper.dll" -Force
    Move-Item "$buildDirectory/bin/Release/ggml-whisper.dll" "$runtimePath/ggml-whisper.dll" -Force
    Move-Item "$buildDirectory/bin/Release/ggml-base-whisper.dll" "$runtimePath/ggml-base-whisper.dll" -Force
    Move-Item "$buildDirectory/bin/Release/ggml-cpu-whisper.dll" "$runtimePath/ggml-cpu-whisper.dll" -Force

    if ($Cuda) {
        Move-Item "$buildDirectory/bin/Release/ggml-cuda-whisper.dll" "$runtimePath/ggml-cuda-whisper.dll" -Force
    }

    if ($Vulkan) {
        Move-Item "$buildDirectory/bin/Release/ggml-vulkan-whisper.dll" "$runtimePath/ggml-vulkan-whisper.dll" -Force
    }
}

function BuildWindowsArm([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindows -Arch "arm64" -Configuration $Configuration
}

function BuildWindowsIntel([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindows -Arch "x64" -Configuration $Configuration
    BuildWindows -Arch "x86" -Configuration $Configuration
}

function BuildWindowsAll([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindows -Arch "arm64" -Configuration $Configuration
    BuildWindows -Arch "arm"   -Configuration $Configuration
    BuildWindows -Arch "x64"   -Cuda $true    -Configuration $Configuration
    BuildWindows -Arch "x64"   -Configuration $Configuration
    BuildWindows -Arch "x86"   -Configuration $Configuration
}

function PackAll([Parameter(Mandatory = $true)] [string]$Version) {

    if (-not(Test-Path "nupkgs")) {
        New-Item -ItemType Directory -Force -Path "nupkgs"
    }
    
    nuget pack runtimes/Whisper.net.Runtime.nuspec -Version $Version -OutputDirectory ./nupkgs
    dotnet pack Whisper.net/Whisper.net.csproj -p:Version=$Version -o ./nupkgs -c Release
    nuget pack runtimes/Whisper.net.Runtime.CoreML.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda.Linux.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda.Windows.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Vulkan.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.OpenVino.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.NoAvx.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.AllRuntimes.nuspec -Version $Version -OutputDirectory ./nupkgs
}
