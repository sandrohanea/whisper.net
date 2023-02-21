#call me from the root of the repo

#if not exist "build" create the directory
if (!(Test-Path "build")) {
    New-Item -ItemType Directory -Force -Path "build"
}

function BuildLinuxX64() {
    Write-Host "Building Linux binaries for x86_64"

    if((Test-Path "build/linux-x64")) {
        Write-Host "Deleting old build files for linux x86_64";
        Remove-Item -Force -Recurse -Path "build/linux-x64"
    }
    
    New-Item -ItemType Directory -Force -Path "build/linux-x64"
    
    #call CMake to generate the makefiles
    cmake -S . -B build/linux-x64
    cmake --build build/linux-x64 --config Release
    
    #copy the binaries to runtimes/linux-x64
    cp build/linux-x64/whisper.cpp/libwhisper.so ./Whisper.net/runtimes/linux-x64/whisper.so
}

function BuildLinuxArm64() {
    Write-Host "Building Linux binaries for arm64"

    if((Test-Path "build/linux-arm64")) {
        Write-Host "Deleting old build files for linux arm64";
        Remove-Item -Force -Recurse -Path "build/linux-arm64"
    }
    
    New-Item -ItemType Directory -Force -Path "build/linux-arm64"
    
    #call CMake to generate the makefiles
    cmake -S . -B build/linux-arm64 -DCMAKE_C_COMPILER=aarch64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=aarch64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=aarch64

    cmake --build build/linux-arm64 --config Release 
    
    #copy the binaries to runtimes/linux-arm64
    cp build/linux-arm64/whisper.cpp/libwhisper.so ./Whisper.net/runtimes/linux-arm64/whisper.so
}

function BuildWebAssembly() {
    Write-Host "Building WebAssembly binaries"

    if((Test-Path "build/webassembly")) {
        Write-Host "Deleting old build files for WebAssembly";
        Remove-Item -Force -Recurse -Path "build/webassembly"
    }
    
    New-Item -ItemType Directory -Force -Path "build/webassembly"
    
    #call CMake to generate the makefiles
    cmake -S . -B build/webassembly -DCMAKE_TOOLCHAIN_FILE=~/emsdk/upstream/emscripten/cmake/Modules/Platform/Emscripten.cmake -DCMAKE_SYSTEM_NAME=Generic -DCMAKE_SYSTEM_PROCESSOR=wasm
    
    cmake --build build/webassembly --config Release 
    
    #copy the binaries to runtimes/webassembly
    cp build/webassembly/whisper.cpp/libwhisper.so ./Whisper.net/runtimes/webassembly/whisper.wasm
}


function BuildLinuxArm() {
    Write-Host "Building Linux binaries for arm"

    if((Test-Path "build/linux-arm")) {
        Write-Host "Deleting old build files for linux arm";
        Remove-Item -Force -Recurse -Path "build/linux-arm"
    }
    
    New-Item -ItemType Directory -Force -Path "build/linux-arm"
    
    #call CMake to generate the makefiles
    cmake -S . -B build/linux-arm -DCMAKE_C_COMPILER=arm-linux-gnueabihf-gcc -DCMAKE_CXX_COMPILER=arm-linux-gnueabihf-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=arm -DCMAKE_SYSTEM_PROCESSOR=arm

    cmake --build build/linux-arm --config Release 
    
    #copy the binaries to runtimes/linux-arm
    cp build/linux-arm/whisper.cpp/libwhisper.so ./Whisper.net/runtimes/linux-arm/whisper.so
}

BuildLinuxArm;
BuildLinuxX64;
BuildLinuxArm64;