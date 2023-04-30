@echo off

set BUILD_TYPE=Release

:windows_x86
cmake -DBUILD_SHARED_LIBS=ON -DCMAKE_BUILD_TYPE=%BUILD_TYPE% -S . -B build\win-x86 -A Win32
cmake --build build\win-x86 --config %BUILD_TYPE%
mkdir Whisper.net.Runtime/win-x86
copy build/win-x86/bin/Release/whisper.dll Whisper.net.Runtime/win-x86/whisper.dll