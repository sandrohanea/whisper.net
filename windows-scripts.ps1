function PackAllNugets([Parameter(Mandatory = $true)] [string]$Version, [Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    New-Item -ItemType Directory -Force -Path "nupkgs"
    nuget pack Whisper.net.Runtime.nuspec -Version $Version -OutputDirectory ./nupkgs
    dotnet pack Whisper.net/Whisper.net.csproj -p:Version=$Version -o ./nupkgs -c $Configuration
    nuget pack Whisper.net.Runtime.CoreML.nuspec -Version $Version -OutputDirectory ./nupkgs
    nuget pack Whisper.net.Runtime.Cuda.nuspec -Version $Version -OutputDirectory ./nupkgs
}

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
        "arm64" = "ARM64"
        "arm"   = "ARM"
    }

    if ($platforms.ContainsKey($Arch)) {
        return $platforms[$Arch]
    }

    return $null
}

function BuildWindowsBase() {
    param(
        [Parameter(Mandatory = $true)] [string]$Arch,
        [Parameter(Mandatory = $false)] [bool]$Cuda = $false,
        [Parameter(Mandatory = $false)] [bool]$Vulkan = $false,
        [Parameter(Mandatory = $false)] [bool]$OpenVino = $false,
        [Parameter(Mandatory = $false)] [string]$Configuration = "Release"
    )
    #if not exist "build" create the directory
    if (!(Test-Path "build")) {
        New-Item -ItemType Directory -Force -Path "build"
    }

    Write-Host "Building Windows binaries for $Arch with cuda: $Cuda"

    
    $platform = Get-MSBuildPlatform $Arch
    if ([string]::IsNullOrEmpty($platform)) {
        Write-Host "Unknown architecture $Arch"
        return
    }
    
    $buildDirectory = "build/win-$Arch"
    $options = @("-S", ".")

    if ($Cuda) {
        $options += "-DGGML_CUDA=1"
        $buildDirectory += "-cuda"
    }

    if ($Vulkan) {
        $options += "-DGGML_VULKAN=1"
        $buildDirectory += "-vulkan"
    }
    
    if ($OpenVino) {
        $options += "-DWHISPER_OPENVINO=1"
        $buildDirectory += "-openvino"
    }

    $options += "-B"
    $options += $buildDirectory
    $options += "-A"
    $options += $platform

    if ((Test-Path $buildDirectory)) {
        Write-Host "Deleting old build files for $buildDirectory";
        Remove-Item -Force -Recurse -Path $buildDirectory
    }

    $cmakePath = (Get-Command cmake -ErrorAction SilentlyContinue).Source
    if ([string]::IsNullOrEmpty($cmakePath)) {
        # CMake is not defined in the system's path, search for it in Visual Studio
        $visualStudioPath = Get-VisualStudioCMakePath
        if ([string]::IsNullOrEmpty($visualStudioPath)) {
            Write-Host "CMake is not found in the system or Visual Studio."
            return
        }
        $env:Path += ";$visualStudioPath"
    }

    New-Item -ItemType Directory -Force -Path $buildDirectory

    # call CMake to generate the makefiles
    
    Write-Host "Running 'cmake $options'"

    cmake $options
    cmake --build $buildDirectory --config $Configuration

    $runtimePath = "./Whisper.net.Runtime"
    if ($Cuda) {
        $runtimePath += ".Cuda"
    }

    if ($Vulkan) {
        $runtimePath += ".Vulkan"
    }

    if ($OpenVino) {
        $runtimePath += ".OpenVino"
    }

    if (-not(Test-Path $runtimePath)) {
        New-Item -ItemType Directory -Force -Path $runtimePath
    }

    $runtimePath += "/win-$Arch"
    
    if (-not(Test-Path $runtimePath)) {
        New-Item -ItemType Directory -Force -Path $runtimePath
    }

    Move-Item "$buildDirectory/bin/Release/whisper.dll" "$runtimePath/whisper.dll" -Force
    Move-Item "$buildDirectory/bin/Release/ggml.dll" "$runtimePath/ggml.dll" -Force
}

function BuildWindowsArm([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindowsBase -Arch "arm64" -Configuration $Configuration;
 #   BuildWindowsBase -Arch "arm" -Configuration $Configuration;
 # Arm build not working anymore with VS
}

function BuildWindowsIntel([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindowsBase -Arch "x64" -Cuda $true -Configuration $Configuration;
    BuildWindowsBase -Arch "x64" -Configuration $Configuration;
    BuildWindowsBase -Arch "x86" -Configuration $Configuration;
}

function BuildWindowsAll([Parameter(Mandatory = $false)] [string]$Configuration = "Release") {
    BuildWindowsBase -Arch "arm64" -Configuration $Configuration;
    BuildWindowsBase -Arch "arm" -Configuration $Configuration;
    BuildWindowsBase -Arch "x64" -Cuda $true -Configuration $Configuration;
    BuildWindowsBase -Arch "x64" -Configuration $Configuration;
    BuildWindowsBase -Arch "x86" -Configuration $Configuration;
}
