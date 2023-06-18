#call me from the root of the repo

#if not exist "build" create the directory
if (!(Test-Path "build")) {
    New-Item -ItemType Directory -Force -Path "build"
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
        [Parameter(Mandatory=$true)] [string]$Arch,
        [Parameter(Mandatory=$false)] [bool]$Cublas = $false,
        [Parameter(Mandatory=$false)] [bool]$Clblast = $false
    )
    Write-Host "Building Windows binaries for $Arch with cublas: $Cublas, and clblast: $Clblast"

    
    $platform = Get-MSBuildPlatform $Arch
    if ([string]::IsNullOrEmpty($platform)) {
        Write-Host "Unknown architecture $Arch"
        return
    }
    
    $buildDirectory = "build/win-$Arch"
    $options = @("-S", ".")

    if ($Cublas) {
        $options += "-DWHISPER_CUBLAS=1"
        $buildDirectory += "-cublas"
    }

    if ($Clblast) {
        $options += "-DWHISPER_CLBLAST=1"
        $buildDirectory += "-clblast"
    }

    $options += "-B"
    $options += $buildDirectory
    $options += "-A"
    $options += $platform

    if((Test-Path $buildDirectory)) {
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
    cmake --build $buildDirectory --config Release

    $runtimePath = "./Whisper.net.Runtime"
    if ($Cublas) {
        $runtimePath += ".Cublas"
    }
    if ($Clblast) {
        $runtimePath += ".Clblast"
    }
    
    if (-not(Test-Path $runtimePath)) {
        New-Item -ItemType Directory -Force -Path $runtimePath
    }

    $runtimePath += "/win-$Arch"
    
    if (-not(Test-Path $runtimePath)) {
        New-Item -ItemType Directory -Force -Path $runtimePath
    }

    Move-Item "$buildDirectory/bin/Release/whisper.dll" "$runtimePath/whisper.dll" -Force
}

function BuildWindowsAll() {
    BuildWindowsBase -Arch "x64";
    BuildWindowsBase -Arch "x86";
    BuildWindowsBase -Arch "arm64";
    BuildWindowsBase -Arch "arm";
    BuildWindowsBase -Arch "x64" -Cublas $true;
    BuildWindowsBase -Arch "x64" -Clblast $true;
}

BuildWindowsAll