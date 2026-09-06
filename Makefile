BUILD_TYPE=Release
ENGINE=whisper
RUNTIME_SUFFIX=$(if $(filter parakeet,$(ENGINE)),.Parakeet,)
RUNTIME_PACKAGE=Whisper.net.Runtime$(RUNTIME_SUFFIX)
ENGINE_PARAMETERS=-DWHISPER_NET_RUNTIME_NAME=$(ENGINE)
CMAKE_PARAMETERS=-DCMAKE_BUILD_TYPE=$(BUILD_TYPE)
COREML_SUPPORT=$(CMAKE_PARAMETERS) -DWHISPER_COREML=ON -DWHISPER_COREML_ALLOW_FALLBACK=ON
AVX_SUPPORT=-DGGML_AVX=ON -DGGML_AVX2=ON -DGGML_FMA=ON -DGGML_F16C=ON
NOAVX_SUPPORT=-DGGML_AVX=OFF -DGGML_AVX2=OFF -DGGML_FMA=OFF -DGGML_F16C=OFF
NDK := $(if $(strip $(NDK_PATH)),$(NDK_PATH),$(shell test -d $(HOME)/Library/Developer/Xamarin/android-sdk-macosx/ndk-bundle && echo $(HOME)/Library/Developer/Xamarin/android-sdk-macosx/ndk-bundle || echo ""))

nuget:
	mkdir -p nupkgs
	nuget pack runtimes/Whisper.net.Runtime.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Metal.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	dotnet pack Whisper.net/Whisper.net.csproj -p:Version=$(VERSION) -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o ./nupkgs -c $(BUILD_TYPE)
	nuget pack runtimes/Whisper.net.Runtime.CoreML.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda.Linux.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda.Windows.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda12.Linux.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda12.Windows.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Cuda12.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Vulkan.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.OpenVino.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.NoAvx.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Metal.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.NoAvx.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda.Linux.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda.Windows.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda12.Linux.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda12.Windows.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Cuda12.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.Runtime.Parakeet.Vulkan.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	nuget pack runtimes/Whisper.net.AllRuntimes.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs

clean:
	rm -rf nupkgs
	rm -rf build
	rm -rf runtimes

android: android_x64 android_x86 android_arm64-v8a

apple_x64: copy_metal macos_x64
apple_arm: copy_metal macos_arm64 ios maccatalyst_arm64  ios_simulator_arm64  tvos_simulator_arm64 tvos

apple_coreml_x64: copy_metal macos_x64_coreml
apple_coreml_arm: copy_metal macos_arm64_coreml ios_coreml  maccatalyst_arm64_coreml ios_simulator_coreml

linux: linux_x64 linux_arm64 linux_arm

linux_noavx: linux_x64_noavx

linux_cuda: linux_x64_cuda

linux_cuda12: linux_x64_cuda12

linux_vulkan: linux_x64_vulkan

copy_metal:
	cp whisper.cpp/ggml/src/ggml-metal/ggml-metal.metal runtimes/$(RUNTIME_PACKAGE).Metal/ggml-metal.metal

 # WASM hack to run under bash as emcmake overrides env variables and cannot run cmake anymore.
wasm:
	/bin/bash -c '\
	  CMAKE_BIN=$$(which cmake); \
	  echo "Using cmake: $$CMAKE_BIN"; \
	  $$CMAKE_BIN --version; \
	  rm -rf build/wasm; \
	  emcmake $$CMAKE_BIN $(ENGINE_PARAMETERS) -S . -B build/wasm -DCMAKE_BUILD_TYPE=$(BUILD_TYPE); \
	  $$CMAKE_BIN --build build/wasm --config $(BUILD_TYPE); \
	  mkdir -p runtimes/$(RUNTIME_PACKAGE)/browser-wasm; \
	  cp build/wasm/whisper.cpp/src/lib$(ENGINE).a ./runtimes/$(RUNTIME_PACKAGE)/browser-wasm/lib$(ENGINE).a; \
	  cp build/wasm/whisper.cpp/ggml/src/libggml-$(ENGINE).a ./runtimes/$(RUNTIME_PACKAGE)/browser-wasm/libggml-$(ENGINE).a; \
	  cp build/wasm/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a ./runtimes/$(RUNTIME_PACKAGE)/browser-wasm/libggml-base-$(ENGINE).a; \
	  cp build/wasm/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a ./runtimes/$(RUNTIME_PACKAGE)/browser-wasm/libggml-cpu-$(ENGINE).a; \
	  '

linux_x64:
	rm -rf build/linux-x64
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-x64 -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 $(AVX_SUPPORT)
	cmake --build build/linux-x64 --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-x64/lib$(ENGINE).so
	cp build/linux-x64/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-x64/libggml-$(ENGINE).so
	cp build/linux-x64/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-x64/libggml-base-$(ENGINE).so
	cp build/linux-x64/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-x64/libggml-cpu-$(ENGINE).so

linux_arm64:
	rm -rf build/linux-arm64
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-arm64 -DCMAKE_C_COMPILER=aarch64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=aarch64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=aarch64
	cmake --build build/linux-arm64 --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/linux-arm64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-arm64/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm64/lib$(ENGINE).so
	cp build/linux-arm64/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm64/libggml-$(ENGINE).so
	cp build/linux-arm64/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm64/libggml-base-$(ENGINE).so
	cp build/linux-arm64/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm64/libggml-cpu-$(ENGINE).so

linux_arm:
	rm -rf build/linux-arm
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-arm -DCMAKE_C_COMPILER=arm-linux-gnueabihf-gcc -DCMAKE_CXX_COMPILER=arm-linux-gnueabihf-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=arm
	cmake --build build/linux-arm --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/linux-arm
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-arm/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm/lib$(ENGINE).so
	cp build/linux-arm/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm/libggml-$(ENGINE).so
	cp build/linux-arm/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm/libggml-base-$(ENGINE).so
	cp build/linux-arm/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/linux-arm/libggml-cpu-$(ENGINE).so

linux_x64_cuda:
	rm -rf build/linux-x64-cuda
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-x64-cuda -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DGGML_CUDA=ON $(AVX_SUPPORT)
	cmake --build build/linux-x64-cuda --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE).Cuda.Linux/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-cuda/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda.Linux/linux-x64/lib$(ENGINE).so
	cp build/linux-x64-cuda/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda.Linux/linux-x64/libggml-$(ENGINE).so
	cp build/linux-x64-cuda/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda.Linux/linux-x64/libggml-base-$(ENGINE).so
	cp build/linux-x64-cuda/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda.Linux/linux-x64/libggml-cpu-$(ENGINE).so
	cp build/linux-x64-cuda/bin/libggml-cuda-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda.Linux/linux-x64/libggml-cuda-$(ENGINE).so

linux_x64_cuda12:
	rm -rf build/linux-x64-cuda12
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-x64-cuda12 -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DGGML_CUDA=ON $(AVX_SUPPORT)
	cmake --build build/linux-x64-cuda12 --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE).Cuda12.Linux/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-cuda12/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda12.Linux/linux-x64/lib$(ENGINE).so
	cp build/linux-x64-cuda12/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda12.Linux/linux-x64/libggml-$(ENGINE).so
	cp build/linux-x64-cuda12/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda12.Linux/linux-x64/libggml-base-$(ENGINE).so
	cp build/linux-x64-cuda12/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda12.Linux/linux-x64/libggml-cpu-$(ENGINE).so
	cp build/linux-x64-cuda12/bin/libggml-cuda-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Cuda12.Linux/linux-x64/libggml-cuda-$(ENGINE).so

linux_x64_noavx:
	rm -rf build/linux-x64-noavx
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-x64-noavx -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 $(NOAVX_SUPPORT)
	cmake --build build/linux-x64-noavx --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE).NoAvx/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-noavx/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).NoAvx/linux-x64/lib$(ENGINE).so
	cp build/linux-x64-noavx/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).NoAvx/linux-x64/libggml-$(ENGINE).so
	cp build/linux-x64-noavx/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).NoAvx/linux-x64/libggml-base-$(ENGINE).so
	cp build/linux-x64-noavx/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).NoAvx/linux-x64/libggml-cpu-$(ENGINE).so


linux_x64_openvino:
	rm -rf build/linux-x64-openvino
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-x64-openvino -DCMAKE_C_COMPILER=x86_64-linux-gnu-gcc -DCMAKE_CXX_COMPILER=x86_64-linux-gnu-g++ -DCMAKE_SYSTEM_NAME=Linux -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DWHISPER_OPENVINO=ON $(AVX_SUPPORT)
	cmake --build build/linux-x64-openvino --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE).OpenVino/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-openvino/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).OpenVino/linux-x64/lib$(ENGINE).so
	cp build/linux-x64-openvino/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).OpenVino/linux-x64/libggml-$(ENGINE).so
	cp build/linux-x64-openvino/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).OpenVino/linux-x64/libggml-base-$(ENGINE).so
	cp build/linux-x64-openvino/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).OpenVino/linux-x64/libggml-cpu-$(ENGINE).so

linux_x64_vulkan:
	rm -rf build/linux-x64-vulkan
	echo "Path is: `$(PATH)`"
	cmake $(ENGINE_PARAMETERS) -S . -B build/linux-x64-vulkan -DGGML_VULKAN=ON -DVulkan_INCLUDE_DIR="$(VULKAN_SDK)/include" -DVulkan_LIBRARY="$(VULKAN_SDK)/lib/libvulkan.so" $(AVX_SUPPORT)
	cmake --build build/linux-x64-vulkan --config $(BUILD_TYPE)
	mkdir -p runtimes/$(RUNTIME_PACKAGE).Vulkan/linux-x64
	echo 'LDD VERSION'
	ldd --version
	cp build/linux-x64-vulkan/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Vulkan/linux-x64/lib$(ENGINE).so
	cp build/linux-x64-vulkan/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Vulkan/linux-x64/libggml-$(ENGINE).so
	cp build/linux-x64-vulkan/bin/libggml-base-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Vulkan/linux-x64/libggml-base-$(ENGINE).so
	cp build/linux-x64-vulkan/bin/libggml-cpu-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Vulkan/linux-x64/libggml-cpu-$(ENGINE).so
	cp build/linux-x64-vulkan/bin/libggml-vulkan-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE).Vulkan/linux-x64/libggml-vulkan-$(ENGINE).so

macos_x64:
	rm -rf build/macos-x64
	cmake $(ENGINE_PARAMETERS) -S . -DCMAKE_OSX_ARCHITECTURES="x86_64" -DGGML_METAL=OFF -B build/macos-x64
	cmake --build build/macos-x64
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/macos-x64
	cp build/macos-x64/bin/lib$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-x64/lib$(ENGINE).dylib
	cp build/macos-x64/bin/libggml-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-x64/libggml-$(ENGINE).dylib
	cp build/macos-x64/bin/libggml-base-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-x64/libggml-base-$(ENGINE).dylib
	cp build/macos-x64/bin/libggml-cpu-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-x64/libggml-cpu-$(ENGINE).dylib
	cp build/macos-x64/bin/libggml-blas-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-x64/libggml-blas-$(ENGINE).dylib

macos_arm64:
	rm -rf build/macos-arm64
	cmake $(ENGINE_PARAMETERS) -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_C_FLAGS="-U__ARM_FEATURE_MATMUL_INT8" -DCMAKE_CXX_FLAGS="-U__ARM_FEATURE_MATMUL_INT8" . -B build/macos-arm64
	cmake --build build/macos-arm64
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/macos-arm64
	cp build/macos-arm64/bin/lib$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-arm64/lib$(ENGINE).dylib
	cp build/macos-arm64/bin/libggml-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-arm64/libggml-$(ENGINE).dylib
	cp build/macos-arm64/bin/libggml-base-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-arm64/libggml-base-$(ENGINE).dylib
	cp build/macos-arm64/bin/libggml-cpu-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-arm64/libggml-cpu-$(ENGINE).dylib
	cp build/macos-arm64/bin/libggml-metal-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-arm64/libggml-metal-$(ENGINE).dylib
	cp build/macos-arm64/bin/libggml-blas-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE)/macos-arm64/libggml-blas-$(ENGINE).dylib

macos_x64_coreml:
	rm -rf build/macos-x64-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_ARCHITECTURES="x86_64" -DGGML_METAL=OFF -S . -B build/macos-x64-coreml
	cmake --build build/macos-x64-coreml
	mkdir -p runtimes/$(RUNTIME_PACKAGE).CoreML/macos-x64
	cp build/macos-x64-coreml/bin/lib$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-x64/lib$(ENGINE).dylib
	cp build/macos-x64-coreml/bin/libggml-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-x64/libggml-$(ENGINE).dylib
	cp build/macos-x64-coreml/bin/lib$(ENGINE).coreml.dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-x64/lib$(ENGINE).coreml.dylib
	cp build/macos-x64-coreml/bin/libggml-base-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-x64/libggml-base-$(ENGINE).dylib
	cp build/macos-x64-coreml/bin/libggml-cpu-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-x64/libggml-cpu-$(ENGINE).dylib
	cp build/macos-x64-coreml/bin/libggml-blas-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-x64/libggml-blas-$(ENGINE).dylib

macos_arm64_coreml:
	rm -rf build/macos-arm64-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_C_FLAGS="-U__ARM_FEATURE_MATMUL_INT8" -DCMAKE_CXX_FLAGS="-U__ARM_FEATURE_MATMUL_INT8"  -S . -B build/macos-arm64-coreml
	cmake --build build/macos-arm64-coreml
	mkdir -p runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64
	cp build/macos-arm64-coreml/bin/lib$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64/lib$(ENGINE).dylib
	cp build/macos-arm64-coreml/bin/lib$(ENGINE).coreml.dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64/lib$(ENGINE).coreml.dylib
	cp build/macos-arm64-coreml/bin/libggml-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64/libggml-$(ENGINE).dylib
	cp build/macos-arm64-coreml/bin/libggml-base-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64/libggml-base-$(ENGINE).dylib
	cp build/macos-arm64-coreml/bin/libggml-cpu-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64/libggml-cpu-$(ENGINE).dylib
	cp build/macos-arm64-coreml/bin/libggml-metal-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64/libggml-metal-$(ENGINE).dylib
	cp build/macos-arm64-coreml/bin/libggml-blas-$(ENGINE).dylib ./runtimes/$(RUNTIME_PACKAGE).CoreML/macos-arm64/libggml-blas-$(ENGINE).dylib

ios:
	rm -rf build/ios
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DBUILD_SHARED_LIBS=OFF -DCMAKE_OSX_SYSROOT="iphoneos" -DCMAKE_SYSTEM_NAME=iOS -S . -B build/ios
	cmake --build build/ios
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/ios-device
	cp build/ios/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-device/lib$(ENGINE).a
	cp build/ios/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-device/libggml-$(ENGINE).a
	cp build/ios/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-device/libggml-base-$(ENGINE).a
	cp build/ios/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-device/libggml-cpu-$(ENGINE).a
	cp build/ios/whisper.cpp/ggml/src/ggml-metal/libggml-metal-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-device/libggml-metal-$(ENGINE).a
	cp build/ios/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-device/libggml-blas-$(ENGINE).a

ios_coreml:
	rm -rf build/ios-coreml
	cmake $(COREML_SUPPORT) -DBUILD_SHARED_LIBS=OFF -DCMAKE_OSX_SYSROOT="iphoneos" -DGGML_METAL=OFF -DCMAKE_SYSTEM_NAME=iOS  -S . -B build/ios-coreml
	cmake --build build/ios-coreml
	mkdir -p runtimes/$(RUNTIME_PACKAGE).CoreML/ios-device
	cp build/ios-coreml/whisper.cpp/src/lib$(ENGINE).coreml.a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-device/lib$(ENGINE).coreml.a
	cp build/ios-coreml/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-device/lib$(ENGINE).a
	cp build/ios-coreml/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-device/libggml-$(ENGINE).a
	cp build/ios-coreml/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-device/libggml-base-$(ENGINE).a
	cp build/ios-coreml/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-device/libggml-cpu-$(ENGINE).a
	cp build/ios-coreml/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-device/libggml-blas-$(ENGINE).a

maccatalyst_arm64:
	rm -rf build/maccatalyst_arm64
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DBUILD_SHARED_LIBS=OFF -S . -B build/maccatalyst_arm64 -DCMAKE_SYSTEM_PROCESSOR=arm -DCMAKE_HOST_SYSTEM_PROCESSOR=arm64 -DGGML_METAL=OFF -DCMAKE_SYSTEM_NAME=Darwin -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_CXX_FLAGS="-target arm64-apple-ios13.1-macabi" -DCMAKE_C_FLAGS="-target arm64-apple-ios13.1-macabi"
	cmake --build build/maccatalyst_arm64
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/maccatalyst
	cp build/maccatalyst_arm64/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/maccatalyst/lib$(ENGINE).a
	cp build/maccatalyst_arm64/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/maccatalyst/libggml-$(ENGINE).a
	cp build/maccatalyst_arm64/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/maccatalyst/libggml-base-$(ENGINE).a
	cp build/maccatalyst_arm64/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/maccatalyst/libggml-cpu-$(ENGINE).a
	cp build/maccatalyst_arm64/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/maccatalyst/libggml-blas-$(ENGINE).a

maccatalyst_arm64_coreml:
	rm -rf build/maccatalyst-arm64-coreml
	cmake $(COREML_SUPPORT) -DBUILD_SHARED_LIBS=OFF -S . -B build/maccatalyst-arm64-coreml -DCMAKE_SYSTEM_PROCESSOR=arm -DCMAKE_HOST_SYSTEM_PROCESSOR=arm64 -DGGML_METAL=OFF -DCMAKE_SYSTEM_NAME=Darwin -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_CXX_FLAGS="-target arm64-apple-ios13.1-macabi" -DCMAKE_C_FLAGS="-target arm64-apple-ios13.1-macabi"
	cmake --build build/maccatalyst-arm64-coreml
	mkdir -p runtimes/$(RUNTIME_PACKAGE).CoreML/maccatalyst
	cp build/maccatalyst-arm64-coreml/whisper.cpp/src/lib$(ENGINE).coreml.a runtimes/$(RUNTIME_PACKAGE).CoreML/maccatalyst/lib$(ENGINE).coreml.a
	cp build/maccatalyst-arm64-coreml/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/maccatalyst/lib$(ENGINE).a
	cp build/maccatalyst-arm64-coreml/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/maccatalyst/libggml-$(ENGINE).a
	cp build/maccatalyst-arm64-coreml/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/maccatalyst/libggml-base-$(ENGINE).a
	cp build/maccatalyst-arm64-coreml/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/maccatalyst/libggml-cpu-$(ENGINE).a
	cp build/maccatalyst-arm64-coreml/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/maccatalyst/libggml-blas-$(ENGINE).a

ios_simulator_coreml:
	rm -rf build/ios-simulator-coreml
	cmake $(COREML_SUPPORT) -DBUILD_SHARED_LIBS=OFF -DCMAKE_OSX_SYSROOT="iphonesimulator" -DGGML_METAL=OFF -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/ios-simulator-coreml
	cmake --build build/ios-simulator-coreml
	mkdir -p runtimes/$(RUNTIME_PACKAGE).CoreML/ios-simulator
	cp build/ios-simulator-coreml/whisper.cpp/src/lib$(ENGINE).coreml.a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-simulator/lib$(ENGINE).coreml.a
	cp build/ios-simulator-coreml/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-simulator/lib$(ENGINE).a
	cp build/ios-simulator-coreml/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-simulator/libggml-$(ENGINE).a
	cp build/ios-simulator-coreml/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-simulator/libggml-base-$(ENGINE).a
	cp build/ios-simulator-coreml/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-simulator/libggml-cpu-$(ENGINE).a
	cp build/ios-simulator-coreml/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE).CoreML/ios-simulator/libggml-blas-$(ENGINE).a

ios_simulator_arm64:
	rm -rf build/ios_simulator_arm64
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DBUILD_SHARED_LIBS=OFF -DCMAKE_OSX_SYSROOT="iphonesimulator" -DGGML_METAL=OFF -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/ios_simulator_arm64
	cmake --build build/ios_simulator_arm64
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/ios-simulator
	cp build/ios_simulator_arm64/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-simulator/lib$(ENGINE).a
	cp build/ios_simulator_arm64/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-simulator/libggml-$(ENGINE).a
	cp build/ios_simulator_arm64/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-simulator/libggml-base-$(ENGINE).a
	cp build/ios_simulator_arm64/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-simulator/libggml-cpu-$(ENGINE).a
	cp build/ios_simulator_arm64/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/ios-simulator/libggml-blas-$(ENGINE).a

tvos_simulator_arm64:
	rm -rf build/tvos_simulator_arm64
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DBUILD_SHARED_LIBS=OFF -DCMAKE_OSX_SYSROOT="appletvsimulator" -DGGML_METAL=OFF -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/tvos_simulator_arm64
	cmake --build build/tvos_simulator_arm64
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/tvos-simulator
	cp build/tvos_simulator_arm64/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-simulator/lib$(ENGINE).a
	cp build/tvos_simulator_arm64/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-simulator/libggml-$(ENGINE).a
	cp build/tvos_simulator_arm64/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-simulator/libggml-base-$(ENGINE).a
	cp build/tvos_simulator_arm64/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-simulator/libggml-cpu-$(ENGINE).a
	cp build/tvos_simulator_arm64/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-simulator/libggml-blas-$(ENGINE).a

tvos:
	rm -rf build/tvos
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DBUILD_SHARED_LIBS=OFF -DCMAKE_OSX_SYSROOT="appletvos" -DCMAKE_SYSTEM_NAME=tvOS -S . -B build/tvos
	cmake --build build/tvos
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/tvos-device
	cp build/tvos/whisper.cpp/src/lib$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-device/lib$(ENGINE).a
	cp build/tvos/whisper.cpp/ggml/src/libggml-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-device/libggml-$(ENGINE).a
	cp build/tvos/whisper.cpp/ggml/src/libggml-base-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-device/libggml-base-$(ENGINE).a
	cp build/tvos/whisper.cpp/ggml/src/libggml-cpu-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-device/libggml-cpu-$(ENGINE).a
	cp build/tvos/whisper.cpp/ggml/src/ggml-blas/libggml-blas-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-device/libggml-blas-$(ENGINE).a
	cp build/tvos/whisper.cpp/ggml/src/ggml-metal/libggml-metal-$(ENGINE).a runtimes/$(RUNTIME_PACKAGE)/tvos-device/libggml-metal-$(ENGINE).a

android_arm64-v8a:
	rm -rf build/android-arm64-v8a
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=arm64-v8a -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -DGGML_OPENMP=OFF -S . -B build/android-arm64-v8a
	cmake --build build/android-arm64-v8a
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/android-arm64-v8a
	cp build/android-arm64-v8a/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/android-arm64-v8a/lib$(ENGINE).so
	cp build/android-arm64-v8a/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/android-arm64-v8a/libggml-$(ENGINE).so
	cp build/android-arm64-v8a/bin/libggml-base-$(ENGINE).so runtimes/$(RUNTIME_PACKAGE)/android-arm64-v8a/libggml-base-$(ENGINE).so
	cp build/android-arm64-v8a/bin/libggml-cpu-$(ENGINE).so runtimes/$(RUNTIME_PACKAGE)/android-arm64-v8a/libggml-cpu-$(ENGINE).so

android_x86:
	rm -rf build/android-x86
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=x86 -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -DGGML_OPENMP=OFF -S . -B build/android-x86
	cmake --build build/android-x86
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/android-x86
	cp build/android-x86/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/android-x86/lib$(ENGINE).so
	cp build/android-x86/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/android-x86/libggml-$(ENGINE).so
	cp build/android-x86/bin/libggml-base-$(ENGINE).so runtimes/$(RUNTIME_PACKAGE)/android-x86/libggml-base-$(ENGINE).so
	cp build/android-x86/bin/libggml-cpu-$(ENGINE).so runtimes/$(RUNTIME_PACKAGE)/android-x86/libggml-cpu-$(ENGINE).so

android_x64:
	rm -rf build/android-x86_64
	cmake $(CMAKE_PARAMETERS) $(ENGINE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=x86_64 -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -DGGML_OPENMP=OFF -S . -B build/android-x86_64
	cmake --build build/android-x86_64
	mkdir -p runtimes/$(RUNTIME_PACKAGE)/android-x86_64
	cp build/android-x86_64/bin/lib$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/android-x86_64/lib$(ENGINE).so
	cp build/android-x86_64/bin/libggml-$(ENGINE).so ./runtimes/$(RUNTIME_PACKAGE)/android-x86_64/libggml-$(ENGINE).so
	cp build/android-x86_64/bin/libggml-base-$(ENGINE).so runtimes/$(RUNTIME_PACKAGE)/android-x86_64/libggml-base-$(ENGINE).so
	cp build/android-x86_64/bin/libggml-cpu-$(ENGINE).so runtimes/$(RUNTIME_PACKAGE)/android-x86_64/libggml-cpu-$(ENGINE).so

xcframework:
	mkdir -p output/lib
	xcrun xcodebuild -create-xcframework -library runtimes/$(RUNTIME_PACKAGE)/ios-device/lib$(ENGINE).dylib -library runtimes/$(RUNTIME_PACKAGE)/ios-simulator/lib$(ENGINE).dylib -library runtimes/$(RUNTIME_PACKAGE)/tvos-device/lib$(ENGINE).dylib -library runtimes/$(RUNTIME_PACKAGE)/tvos-simulator/lib$(ENGINE).dylib -library runtimes/$(RUNTIME_PACKAGE)/macos/lib$(ENGINE).dylib -library runtimes/$(RUNTIME_PACKAGE)/maccatalyst/lib$(ENGINE).dylib 	-output output/lib/$(ENGINE).xcframework
