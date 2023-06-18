#call me from the root of the repo

#if not exist "build" create the directory
if (!(Test-Path "build")) {
    New-Item -ItemType Directory -Force -Path "build"
}

function BuildLinuxBase() {
    param(
        [Parameter(Mandatory=$true)] [string]$Arch,
        [Parameter(Mandatory=$false)] [bool]$Cublas = $false,
        [Parameter(Mandatory=$false)] [bool]$Clblast = $false
    )
    Write-Host "Building Linux binaries for $Arch with cublas: $Cublas, and clblast: $Clblast"

    
    $buildDirectory = "build/linux-$Arch"
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
    if ($Arch -eq "arm64") {
        $options += "-DCMAKE_C_COMPILER=aarch64-linux-gnu-gcc"
        $options += "-DCMAKE_CXX_COMPILER=aarch64-linux-gnu-g++"
        $options += "-DCMAKE_SYSTEM_NAME=Linux"
        $options += "-DCMAKE_SYSTEM_PROCESSOR=aarch64"
    }
    elseif ($Arch -eq "arm") {
        $options += "-DCMAKE_C_COMPILER=arm-linux-gnueabihf-gcc"
        $options += "-DCMAKE_CXX_COMPILER=arm-linux-gnueabihf-g++"
        $options += "-DCMAKE_SYSTEM_NAME=Linux"
        $options += "-DCMAKE_SYSTEM_PROCESSOR=arm"
    }
    elseif ($Arch -ne "x64") {
        Write-Host "Unsupported architecture: $Arch"
        return
    }

    if((Test-Path $buildDirectory)) {
        Write-Host "Deleting old build files for $buildDirectory";
        Remove-Item -Force -Recurse -Path $buildDirectory
    }

     $cmakePath = (Get-Command cmake -ErrorAction SilentlyContinue).Source
    if ([string]::IsNullOrEmpty($cmakePath)) {
        Write-Host "CMake is not found in the system path."
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

    $runtimePath += "/linux-$Arch"
    
    if (-not(Test-Path $runtimePath)) {
        New-Item -ItemType Directory -Force -Path $runtimePath
    }

    Move-Item "$buildDirectory/whisper.cpp/libwhisper.so" "$runtimePath/whisper.so" -Force
}

function BuildLinuxAll() {
    BuildLinuxBase -Arch "x64";
    BuildLinuxBase -Arch "arm64";
    BuildLinuxBase -Arch "arm";
    #WIP Cublas and Clblast
    #BuildLinuxBase -Arch "x64" -Cublas $true;
    #BuildLinuxBase -Arch "x64" -Clblast $true;
}

BuildLinuxAll