
function Get-VisualStudioCMakePath() {
    $vsWherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vsWherePath)) {
        return $null
    }

    $vsWhereOutput = & $vsWherePath -latest -requires Microsoft.Component.MSBuild -property installationPath
    if (-not ([string]::IsNullOrEmpty($vsWhereOutput))) {
        $cmakePath = Join-Path $vsWhereOutput 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin'
        if (Test-Path $cmakePath) {
            return $cmakePath
        }
    }

    return $null
}

function Get-MSBuildPlatform($Arch) {
    $platforms = @{
        "x64"   = "x64"
        "x86"   = "Win32"
    }

    if ($platforms.ContainsKey($Arch)) {
        return $platforms[$Arch]
    }

    return $null
}

function BuildWindows() {
    param(
        [Parameter(Mandatory = $true)] [string]$Arch,
        [Parameter(Mandatory = $false)] [bool]$Cuda = $false,
        [Parameter(Mandatory = $false)] [bool]$Vulkan = $false,
        [Parameter(Mandatory = $false)] [bool]$OpenVino = $false,
        [Parameter(Mandatory = $false)] [bool]$NoAvx = $false,
        [Parameter(Mandatory = $false)] [string]$Configuration = "Release",
        [Parameter(Mandatory = $false)] [string]$CudaVersion = "13",
        [Parameter(Mandatory = $false)]
        [ValidateSet("Whisper", "Parakeet")]
        [string]$Engine = "Whisper"
    )

    # Ensure the build directory exists
    if (!(Test-Path "build")) {
        New-Item -ItemType Directory -Force -Path "build"
    }

    $engineName = $Engine.ToLowerInvariant()
    Write-Host "Building $Engine Windows binaries for $Arch (using Clang + Ninja) with cuda: $Cuda"

    $buildDirectory = "build/win-$Arch"
    $options = @(
        "-S", ".",
        "-DGGML_NATIVE=OFF"
    )
    $runtimePath = if ($Engine -eq "Parakeet") {
        "./runtimes/Whisper.net.Runtime.Parakeet"
    } else {
        "./runtimes/Whisper.net.Runtime"
    }

    if ($Engine -eq "Parakeet") {
        $buildDirectory += "-parakeet"
        $options += "-DWHISPER_NET_RUNTIME_NAME=parakeet"
    }

    $avxOptions = @("-DGGML_AVX=ON", "-DGGML_AVX2=ON", "-DGGML_FMA=ON", "-DGGML_F16C=ON")

    if ($NoAvx) {
        $avxOptions = @("-DGGML_AVX=OFF", "-DGGML_AVX2=OFF", "-DGGML_FMA=OFF", "-DGGML_F16C=OFF")
        $buildDirectory += "-noavx"
        $runtimePath += ".NoAvx"
    }

    if($Arch -eq "arm64") {
        $options += "-G"
        $options += "Ninja Multi-Config"
        $options += "-DCMAKE_TOOLCHAIN_FILE=cmake/$Arch-windows-llvm.cmake"
    }
    else {
        $platform = Get-MSBuildPlatform $Arch
        $options += "-A"
        $options += $platform

        # Add AVX flags
        $options += $avxOptions

        if ($platform -eq "Win32")
        {
            $options += "-DGGML_BMI2=OFF";
        }
    }

    if ($Cuda) {
        $options += "-DGGML_CUDA=1"
        if ($CudaVersion -eq "12") {
            $buildDirectory += "-cuda12"
            $runtimePath += ".Cuda12.Windows"
        }
        else {
            $buildDirectory += "-cuda"
            $runtimePath += ".Cuda.Windows"
        }
    }

    if ($Vulkan) {
        $options += "-DGGML_VULKAN=1"
        $buildDirectory += "-vulkan"
        $runtimePath += ".Vulkan"
    }

    if ($OpenVino) {
        if ($Engine -eq "Parakeet") {
            throw "OpenVINO is not supported by the Parakeet engine."
        }
        $options += "-DWHISPER_OPENVINO=1"
        $buildDirectory += "-openvino"
        $runtimePath += ".OpenVino"
    }


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
    Move-Item "$buildDirectory/bin/$Configuration/$engineName.dll" "$runtimePath/$engineName.dll" -Force
    Move-Item "$buildDirectory/bin/$Configuration/ggml-$engineName.dll" "$runtimePath/ggml-$engineName.dll" -Force
    Move-Item "$buildDirectory/bin/$Configuration/ggml-base-$engineName.dll" "$runtimePath/ggml-base-$engineName.dll" -Force
    Move-Item "$buildDirectory/bin/$Configuration/ggml-cpu-$engineName.dll" "$runtimePath/ggml-cpu-$engineName.dll" -Force

    if ($Cuda) {
        Move-Item "$buildDirectory/bin/$Configuration/ggml-cuda-$engineName.dll" "$runtimePath/ggml-cuda-$engineName.dll" -Force
    }

    if ($Vulkan) {
        Move-Item "$buildDirectory/bin/$Configuration/ggml-vulkan-$engineName.dll" "$runtimePath/ggml-vulkan-$engineName.dll" -Force
    }

    if ($Configuration -eq "DEBUG")
    {
        # We copy the PDB files for DEBUG as well
        Move-Item "$buildDirectory/bin/$Configuration/$engineName.pdb" "$runtimePath/$engineName.pdb" -Force
        Move-Item "$buildDirectory/bin/$Configuration/ggml-$engineName.pdb" "$runtimePath/ggml-$engineName.pdb" -Force
        Move-Item "$buildDirectory/bin/$Configuration/ggml-base-$engineName.pdb" "$runtimePath/ggml-base-$engineName.pdb" -Force
        Move-Item "$buildDirectory/bin/$Configuration/ggml-cpu-$engineName.pdb" "$runtimePath/ggml-cpu-$engineName.pdb" -Force
        if ($Cuda) {
            Move-Item "$buildDirectory/bin/$Configuration/ggml-cuda-$engineName.pdb" "$runtimePath/ggml-cuda-$engineName.pdb" -Force
        }
        if ($Vulkan) {
            Move-Item "$buildDirectory/bin/$Configuration/ggml-vulkan-$engineName.pdb" "$runtimePath/ggml-vulkan-$engineName.pdb" -Force
        }
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
    BuildWindows -Arch "x64"   -Cuda $true    -Configuration $Configuration -CudaVersion "12"
    BuildWindows -Arch "x64"   -Configuration $Configuration
    BuildWindows -Arch "x86"   -Configuration $Configuration
}

function BuildWindowsParakeetArm([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindows -Arch "arm64" -Configuration $Configuration -Engine "Parakeet"
}

function BuildWindowsParakeetIntel([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindows -Arch "x64" -Configuration $Configuration -Engine "Parakeet"
    BuildWindows -Arch "x86" -Configuration $Configuration -Engine "Parakeet"
}

function BuildWindowsParakeetAll([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindows -Arch "arm64" -Configuration $Configuration -Engine "Parakeet"
    BuildWindows -Arch "x64" -Cuda $true -Configuration $Configuration -Engine "Parakeet"
    BuildWindows -Arch "x64" -Cuda $true -Configuration $Configuration -CudaVersion "12" -Engine "Parakeet"
    BuildWindows -Arch "x64" -Vulkan $true -Configuration $Configuration -Engine "Parakeet"
    BuildWindows -Arch "x64" -NoAvx $true -Configuration $Configuration -Engine "Parakeet"
    BuildWindows -Arch "x64" -Configuration $Configuration -Engine "Parakeet"
    BuildWindows -Arch "x86" -Configuration $Configuration -Engine "Parakeet"
}

function PackAll([Parameter(Mandatory = $true)] [string]$Version) {

    if (-not(Test-Path "nupkgs")) {
        New-Item -ItemType Directory -Force -Path "nupkgs"
    }

    Get-ChildItem "runtimes/Whisper.net.Runtime.Parakeet*/*.targets" | ForEach-Object {
        $targetFile = $_
        $targetXml = [xml](Get-Content $targetFile.FullName -Raw)
        $targetXml.SelectNodes("//*[@Include]") | ForEach-Object {
            $assetPath = $_.Include.Replace('$(MSBuildThisFileDirectory)', "$($targetFile.DirectoryName)/")
            if (-not (Test-Path $assetPath -PathType Leaf)) {
                throw "Missing Parakeet runtime asset '$assetPath' referenced by '$($targetFile.FullName)'."
            }
        }
    }

    nuget pack runtimes/Whisper.net.Runtime.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Metal.nuspec -Version $Version -OutputDirectory ./nupkgs
    dotnet pack Whisper.net/Whisper.net.csproj -p:Version=$Version -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o ./nupkgs -c Release
    nuget pack runtimes/Whisper.net.Runtime.CoreML.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda.Linux.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda.Windows.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda12.Linux.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda12.Windows.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Cuda12.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Vulkan.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.OpenVino.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.NoAvx.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Metal.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.NoAvx.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda.Linux.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda.Windows.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda12.Linux.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda12.Windows.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda12.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.Runtime.Parakeet.Vulkan.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack runtimes/Whisper.net.AllRuntimes.nuspec -Version $Version -OutputDirectory ./nupkgs
}
