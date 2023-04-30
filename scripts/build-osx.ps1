#call me from the root of the repo

#if not exist "build" create the directory
if (!(Test-Path "build")) {
    New-Item -ItemType Directory -Force -Path "build"
}

function BuildOsxX64() {
    Write-Host "Building OSX binaries for x86_64"

    if((Test-Path "build/osx-x64")) {
        Write-Host "Deleting old build files for osx x86_64";
        Remove-Item -Force -Recurse -Path "build/osx-x64"
    }
    
    New-Item -ItemType Directory -Force -Path "build/osx-x64"
    
    #call CMake to generate the makefiles
    cmake -S . -B build/osx-x64
    cmake --build build/osx-x64 --config Release
    
    #copy the binaries to runtimes/osx-x64
    cp build/osx-x64/whisper.cpp/libwhisper.dylib ./Whisper.net.Runtime/osx-x64/whisper.dylib
}

function BuildOsxArm64() {
    Write-Host "Building OSX binaries for arm64"

    if((Test-Path "build/osx-arm64")) {
        Write-Host "Deleting old build files for osx arm64";
        Remove-Item -Force -Recurse -Path "build/osx-arm64"
    }
    
    New-Item -ItemType Directory -Force -Path "build/osx-arm64"
    
    #call CMake to generate the makefiles
    cmake -S . -B build/osx-arm64 -DCMAKE_OSX_ARCHITECTURES=arm64

    cmake --build build/osx-arm64 --config Release 
    
    #copy the binaries to runtimes/osx-arm64
    cp build/osx-arm64/whisper.cpp/libwhisper.dylib ./Whisper.net.Runtime/osx-arm64/whisper.dylib
}

BuildOsxArm64
BuildOsxX64