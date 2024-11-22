BUILD_TYPE=Release
CMAKE_PARAMETERS=-DCMAKE_BUILD_TYPE=$(BUILD_TYPE)
COREML_SUPPORT=$(CMAKE_PARAMETERS) -DWHISPER_COREML=ON -DWHISPER_COREML_ALLOW_FALLBACK=ON
AVX_SUPPORT=-DGGML_AVX=ON -DGGML_AVX2=ON -DGGML_FMA=ON -DGGML_F16C=ON
NOAVX_SUPPORT=-DGGML_AVX=OFF -DGGML_AVX2=OFF -DGGML_FMA=OFF -DGGML_F16C=OFF
NDK := $(if $(strip $(NDK_PATH)),$(NDK_PATH),$(shell test -d $(HOME)/Library/Developer/Xamarin/android-sdk-macosx/ndk-bundle && echo $(HOME)/Library/Developer/Xamarin/android-sdk-macosx/ndk-bundle || echo ""))

nuget:
	mkdir -p nupkgs
	nuget pack runtimes/Whisper.net.Runtime.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	dotnet pack Whisper.net/Whisper.net.csproj -p:Version=$(VERSION) -o ./nupkgs -c $(BUILD_TYPE)
	nuget pack runtimes/Whisper.net.Runtime.CoreML.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda.Linux.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda.Windows.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Vulkan.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.OpenVino.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.NoAvx.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.AllRuntimes.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs

clean:
	rm -rf nupkgs
	rm -rf build
	rm -rf runtimes

android: android_x64 android_x86 android_arm64-v8a

apple_x64: copy_metal macos_x64
apple_arm: macos_arm64 ios maccatalyst_arm64  ios_simulator_arm64  tvos_simulator_arm64 tvos

apple_coreml_x64: copy_metal_coreml macos_x64_coreml
apple_coreml_arm: macos_arm64_coreml ios_coreml  maccatalyst_arm64_coreml ios_simulator_coreml tvos_simulator_coreml tvos_coreml

linux: linux_x64 linux_arm64 linux_arm

linux_noavx: linux_x64_noavx

linux_cuda: linux_x64_cuda

linux_vulkan: linux_x64_vulkan

copy_metal:
	cp whisper.cpp/ggml/src/ggml-metal.metal runtimes/Whisper.net.Runtime/ggml-metal.metal

copy_metal_coreml:
	cp whisper.cpp/ggml/src/ggml-metal.metal runtimes/Whisper.net.Runtime.CoreML/ggml-metal.metal

wasm:
	rm -rf build/wasm
	emcmake cmake -S . -B build/wasm -DCMAKE_BUILD_TYPE=$(BUILD_TYPE)
	cmake --build build/wasm --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime/browser-wasm
	cp build/wasm/whisper.cpp/src/libwhisper.a ./runtimes/Whisper.net.Runtime/browser-wasm/libwhisper.a
	cp build/wasm/whisper.cpp/ggml/src/libggml-whisper.a ./runtimes/Whisper.net.Runtime/browser-wasm/libggml-whisper.a

linux_x64:
	rm -rf build/linux-x64
	cmake -S . -B build/linux-x64 -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 $(AVX_SUPPORT)
	cmake --build build/linux-x64 --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime/linux-x64/libwhisper.so
	cp build/linux-x64/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime/linux-x64/libggml-whisper.so

linux_arm64:
	rm -rf build/linux-arm64
	cmake -S . -B build/linux-arm64 -DCMAKE_C_COMPILER=aarch64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=aarch64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=aarch64
	cmake --build build/linux-arm64 --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime/linux-arm64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-arm64/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime/linux-arm64/libwhisper.so
	cp build/linux-arm64/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime/linux-arm64/libggml-whisper.so

linux_arm:
	rm -rf build/linux-arm
	cmake -S . -B build/linux-arm -DCMAKE_C_COMPILER=arm-linux-gnueabihf-gcc -DCMAKE_CXX_COMPILER=arm-linux-gnueabihf-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=arm
	cmake --build build/linux-arm --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime/linux-arm
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-arm/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime/linux-arm/libwhisper.so
	cp build/linux-arm/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime/linux-arm/libggml-whisper.so

linux_x64_cuda:
	rm -rf build/linux-x64-cuda
	cmake -S . -B build/linux-x64-cuda -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DGGML_CUDA=ON $(AVX_SUPPORT)
	cmake --build build/linux-x64-cuda --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime.Cuda.Linux/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-cuda/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime.Cuda.Linux/linux-x64/libwhisper.so
	cp build/linux-x64-cuda/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime.Cuda.Linux/linux-x64/libggml-whisper.so

linux_x64_noavx:
	rm -rf build/linux-x64-noavx
	cmake -S . -B build/linux-x64-noavx -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 $(NOAVX_SUPPORT)
	cmake --build build/linux-x64-noavx --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime.NoAvx/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-noavx/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime.NoAvx/linux-x64/libwhisper.so
	cp build/linux-x64-noavx/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime.NoAvx/linux-x64/libggml-whisper.so


linux_x64_openvino:
	rm -rf build/linux-x64-openvino
	cmake -S . -B build/linux-x64-openvino -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DWHISPER_OPENVINO=ON $(AVX_SUPPORT)
	cmake --build build/linux-x64-openvino --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime.OpenVino/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-openvino/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime.OpenVino/linux-x64/libwhisper.so
	cp build/linux-x64-openvino/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime.OpenVino/linux-x64/libggml-whisper.so

linux_x64_vulkan:
	rm -rf build/linux-x64-vulkan
	cmake -S . -B build/linux-x64-vulkan -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DGGML_VULKAN=ON $(AVX_SUPPORT)
	cmake --build build/linux-x64-vulkan --config $(BUILD_TYPE)
	mkdir -p runtimes/Whisper.net.Runtime.Vulkan/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-vulkan/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime.Vulkan/linux-x64/libwhisper.so
	cp build/linux-x64-vulkan/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime.Vulkan/linux-x64/libggml-whisper.so

macos_x64:
	rm -rf build/macos-x64
	cmake -S . -DCMAKE_OSX_ARCHITECTURES="x86_64" -DGGML_METAL=OFF -B build/macos-x64
	cmake --build build/macos-x64
	mkdir -p runtimes/Whisper.net.Runtime/macos-x64
	cp build/macos-x64/whisper.cpp/src/libwhisper.dylib ./runtimes/Whisper.net.Runtime/macos-x64/libwhisper.dylib
	cp build/macos-x64/whisper.cpp/ggml/src/libggml-whisper.dylib ./runtimes/Whisper.net.Runtime/macos-x64/libggml-whisper.dylib

macos_arm64:
	rm -rf build/macos-arm64
	cmake -DCMAKE_OSX_ARCHITECTURES="arm64" -S . -B build/macos-arm64
	cmake --build build/macos-arm64
	mkdir -p runtimes/Whisper.net.Runtime/macos-arm64
	cp build/macos-arm64/whisper.cpp/src/libwhisper.dylib ./runtimes/Whisper.net.Runtime/macos-arm64/libwhisper.dylib
	cp build/macos-arm64/whisper.cpp/ggml/src/libggml-whisper.dylib ./runtimes/Whisper.net.Runtime/macos-arm64/libggml-whisper.dylib

macos_x64_coreml:
	rm -rf build/macos-x64-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_ARCHITECTURES="x86_64" -DGGML_METAL=OFF -S . -B build/macos-x64-coreml
	cmake --build build/macos-x64-coreml
	mkdir -p runtimes/Whisper.net.Runtime.CoreML/macos-x64
	cp build/macos-x64-coreml/whisper.cpp/src/libwhisper.dylib ./runtimes/Whisper.net.Runtime.CoreML/macos-x64/libwhisper.dylib
	cp build/macos-x64-coreml/whisper.cpp/ggml/src/libggml-whisper.dylib ./runtimes/Whisper.net.Runtime.CoreML/macos-x64/libggml-whisper.dylib
	cp build/macos-x64-coreml/whisper.cpp/src/libwhisper.coreml.dylib ./runtimes/Whisper.net.Runtime.CoreML/macos-x64/libwhisper.coreml.dylib

macos_arm64_coreml:
	rm -rf build/macos-arm64-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_ARCHITECTURES="arm64" -S . -B build/macos-arm64-coreml
	cmake --build build/macos-arm64-coreml
	mkdir -p runtimes/Whisper.net.Runtime.CoreML/macos-arm64
	cp build/macos-arm64-coreml/whisper.cpp/src/libwhisper.dylib ./runtimes/Whisper.net.Runtime.CoreML/macos-arm64/libwhisper.dylib
	cp build/macos-arm64-coreml/whisper.cpp/src/libwhisper.coreml.dylib ./runtimes/Whisper.net.Runtime.CoreML/macos-arm64/libwhisper.coreml.dylib
	cp build/macos-arm64-coreml/whisper.cpp/ggml/src/libggml-whisper.dylib ./runtimes/Whisper.net.Runtime.CoreML/macos-arm64/libggml-whisper.dylib

ios:
	rm -rf build/ios
	cmake $(CMAKE_PARAMETERS) -DCMAKE_OSX_SYSROOT="iphoneos" -S . -B build/ios
	cmake --build build/ios
	mkdir -p runtimes/Whisper.net.Runtime/ios-device
	cp build/ios/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime/ios-device/libwhisper.dylib
	cp build/ios/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime/ios-device/libggml-whisper.dylib

ios_coreml:
	rm -rf build/ios-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_ARCHITECTURES="arm64" -DGGML_METAL=OFF -DCMAKE_OSX_SYSROOT="iphoneos" -S . -B build/ios-coreml
	cmake --build build/ios-coreml
	mkdir -p runtimes/Whisper.net.Runtime.CoreML/ios-device
	cp build/ios-coreml/whisper.cpp/src/libwhisper.coreml.dylib runtimes/Whisper.net.Runtime.CoreML/ios-device/libwhisper.coreml.dylib
	cp build/ios-coreml/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime.CoreML/ios-device/libwhisper.dylib
	cp build/ios-coreml/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime.CoreML/ios-device/libggml-whisper.dylib

maccatalyst_arm64:
	rm -rf build/maccatalyst_arm64
	cmake $(CMAKE_PARAMETERS)  -S . -B build/maccatalyst_arm64 -DCMAKE_SYSTEM_PROCESSOR=arm -DCMAKE_HOST_SYSTEM_PROCESSOR=arm64 -DGGML_METAL=OFF -DCMAKE_SYSTEM_NAME=Darwin -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_CXX_FLAGS="-target arm64-apple-ios13.1-macabi" -DCMAKE_C_FLAGS="-target arm64-apple-ios13.1-macabi"
	cmake --build build/maccatalyst_arm64
	mkdir -p runtimes/Whisper.net.Runtime/maccatalyst
	cp build/maccatalyst_arm64/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime/maccatalyst/libwhisper.dylib
	cp build/maccatalyst_arm64/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime/maccatalyst/libggml-whisper.dylib

maccatalyst_arm64_coreml:
	rm -rf build/maccatalyst-arm64-coreml
	cmake $(COREML_SUPPORT)  -S . -B build/maccatalyst-arm64-coreml -DCMAKE_SYSTEM_PROCESSOR=arm -DCMAKE_HOST_SYSTEM_PROCESSOR=arm64 -DGGML_METAL=OFF -DCMAKE_SYSTEM_NAME=Darwin -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_CXX_FLAGS="-target arm64-apple-ios13.1-macabi" -DCMAKE_C_FLAGS="-target arm64-apple-ios13.1-macabi"
	cmake --build build/maccatalyst-arm64-coreml
	mkdir -p runtimes/Whisper.net.Runtime.CoreML/maccatalyst
	cp build/maccatalyst-arm64-coreml/whisper.cpp/src/libwhisper.coreml.dylib runtimes/Whisper.net.Runtime.CoreML/maccatalyst/libwhisper.coreml.dylib
	cp build/maccatalyst-arm64-coreml/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime.CoreML/maccatalyst/libwhisper.dylib
	cp build/maccatalyst-arm64-coreml/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime.CoreML/maccatalyst/libggml-whisper.dylib

ios_simulator_coreml:
	rm -rf build/ios-simulator-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_SYSROOT="iphonesimulator" -DGGML_METAL=OFF -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/ios-simulator-coreml
	cmake --build build/ios-simulator-coreml
	mkdir -p runtimes/Whisper.net.Runtime.CoreML/ios-simulator
	cp build/ios-simulator-coreml/whisper.cpp/src/libwhisper.coreml.dylib runtimes/Whisper.net.Runtime.CoreML/ios-simulator/libwhisper.coreml.dylib
	cp build/ios-simulator-coreml/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime.CoreML/ios-simulator/libwhisper.dylib
	cp build/ios-simulator-coreml/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime.CoreML/ios-simulator/libggml-whisper.dylib

ios_simulator_arm64:
	rm -rf build/ios_simulator_arm64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_OSX_SYSROOT="iphonesimulator" -DGGML_METAL=OFF -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/ios_simulator_arm64
	cmake --build build/ios_simulator_arm64
	mkdir -p runtimes/Whisper.net.Runtime/ios-simulator
	cp build/ios_simulator_arm64/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime/ios-simulator/libwhisper.dylib
	cp build/ios_simulator_arm64/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime/ios-simulator/libggml-whisper.dylib

tvos_simulator_arm64:
	rm -rf build/tvos_simulator_arm64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_OSX_SYSROOT="appletvsimulator" -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/tvos_simulator_arm64
	cmake --build build/tvos_simulator_arm64
	mkdir -p runtimes/Whisper.net.Runtime/tvos-simulator
	cp build/tvos_simulator_arm64/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime/tvos-simulator/libwhisper.dylib

tvos:
	rm -rf build/tvos
	cmake $(CMAKE_PARAMETERS) -DCMAKE_OSX_SYSROOT="appletvos" -DCMAKE_OSX_ARCHITECTURES="arm64" -S . -B build/tvos
	cmake --build build/tvos
	mkdir -p runtimes/Whisper.net.Runtime/tvos-device
	cp build/tvos/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime/tvos-device/libwhisper.dylib
	cp build/tvos/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime/tvos-device/libggml-whisper.dylib

tvos_coreml:
	rm -rf build/tvos-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_SYSROOT="appletvos" -DGGML_METAL=OFF -DCMAKE_OSX_ARCHITECTURES="arm64" -S . -B build/tvos-coreml
	cmake --build build/tvos-coreml
	mkdir -p runtimes/Whisper.net.Runtime.CoreML/tvos-device
	cp build/tvos-coreml/whisper.cpp/src/libwhisper.coreml.dylib runtimes/Whisper.net.Runtime.CoreML/tvos-device/libwhisper.coreml.dylib
	cp build/tvos-coreml/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime.CoreML/tvos-device/libwhisper.dylib
	cp build/tvos-coreml/whisper.cpp/ggml/src/libggml-whisper.dylib  runtimes/Whisper.net.Runtime.CoreML/tvos-device/libggml-whisper.dylib

tvos_simulator_coreml:
	rm -rf build/tvos-simulator-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_SYSROOT="appletvsimulator" -DGGML_METAL=OFF -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/tvos-simulator-coreml
	cmake --build build/tvos-simulator-coreml
	mkdir -p runtimes/Whisper.net.Runtime.CoreML/tvos-simulator
	cp build/tvos-simulator-coreml/whisper.cpp/src/libwhisper.coreml.dylib runtimes/Whisper.net.Runtime.CoreML/tvos-simulator/libwhisper.coreml.dylib
	cp build/tvos-simulator-coreml/whisper.cpp/src/libwhisper.dylib runtimes/Whisper.net.Runtime.CoreML/tvos-simulator/libwhisper.dylib
	cp build/tvos-simulator-coreml/whisper.cpp/ggml/src/libggml-whisper.dylib runtimes/Whisper.net.Runtime.CoreML/tvos-simulator/libggml-whisper.dylib

android_arm64-v8a:
	rm -rf build/android-arm64-v8a
	cmake $(CMAKE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=arm64-v8a -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -DGGML_OPENMP=OFF -S . -B build/android-arm64-v8a
	cmake --build build/android-arm64-v8a
	mkdir -p runtimes/Whisper.net.Runtime/android-arm64-v8a
	cp build/android-arm64-v8a/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime/android-arm64-v8a/libwhisper.so
	cp build/android-arm64-v8a/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime/android-arm64-v8a/libggml-whisper.so

android_x86:
	rm -rf build/android-x86
	cmake $(CMAKE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=x86 -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -DGGML_OPENMP=OFF -S . -B build/android-x86
	cmake --build build/android-x86
	mkdir -p runtimes/Whisper.net.Runtime/android-x86
	cp build/android-x86/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime/android-x86/libwhisper.so
	cp build/android-x86/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime/android-x86/libggml-whisper.so

android_x64:
	rm -rf build/android-x86_64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=x86_64 -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -DGGML_OPENMP=OFF -S . -B build/android-x86_64
	cmake --build build/android-x86_64
	mkdir -p runtimes/Whisper.net.Runtime/android-x86_64
	cp build/android-x86_64/whisper.cpp/src/libwhisper.so ./runtimes/Whisper.net.Runtime/android-x86_64/libwhisper.so
	cp build/android-x86_64/whisper.cpp/ggml/src/libggml-whisper.so ./runtimes/Whisper.net.Runtime/android-x86_64/libggml-whisper.so

xcframework:
	mkdir -p output/lib
	xcrun xcodebuild -create-xcframework -library runtimes/Whisper.net.Runtime/ios-device/libwhisper.dylib -library runtimes/Whisper.net.Runtime/ios-simulator/libwhisper.dylib -library runtimes/Whisper.net.Runtime/tvos-device/libwhisper.dylib -library runtimes/Whisper.net.Runtime/tvos-simulator/libwhisper.dylib -library runtimes/Whisper.net.Runtime/macos/libwhisper.dylib -library runtimes/Whisper.net.Runtime/maccatalyst/libwhisper.dylib -output output/lib/whisper.xcframework
