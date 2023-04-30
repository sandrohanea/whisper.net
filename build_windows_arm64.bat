@echo off

set BUILD_TYPE=Release

:windows_arm64
cmake -DBUILD_SHARED_LIBS=ON -DCMAKE_BUILD_TYPE=%BUILD_TYPE% -S . -B build\win-arm64 -A ARM64
cmake --build build\win-arm64 --config %BUILD_TYPE%
mkdir Whisper.net.Runtime/win-arm64
copy build/win-arm64/bin/Release/whisper.dll Whisper.net.Runtime/win-arm64/whisper.dll
