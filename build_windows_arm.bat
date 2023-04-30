@echo off

set BUILD_TYPE=Release

:windows_arm
cmake -DBUILD_SHARED_LIBS=ON -DCMAKE_BUILD_TYPE=%BUILD_TYPE% -S . -B build\win-arm -A ARM
cmake --build build\win-arm --config %BUILD_TYPE%
mkdir Whisper.net.Runtime/win-arm
copy build/win-arm/bin/Release/whisper.dll Whisper.net.Runtime/win-arm/whisper.dll