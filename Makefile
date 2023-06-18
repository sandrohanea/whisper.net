BUILD_TYPE=Release
VERSION=1.4.4
CMAKE_PARAMETERS=-DCMAKE_BUILD_TYPE=$(BUILD_TYPE)
COREML_SUPPORT=$(CMAKE_PARAMETERS) -DWHISPER_COREML=ON -DWHISPER_COREML_ALLOW_FALLBACK=ON
NDK :=
ifeq ($(strip $(NDK_PATH)),)
    ifeq ($(shell test -d $(HOME)/Library/Developer/Xamarin/android-sdk-macosx/ndk-bundle && echo yes),yes)
        NDK := $(HOME)/Library/Developer/Xamarin/android-sdk-macosx/ndk-bundle
    else
        $(error NDK_PATH not defined and NDK not found at default location on Mac.)
    endif
else
    NDK := $(strip $(NDK_PATH))
endif

nuget:
	mkdir -p nupkgs
	nuget pack Whisper.net.Runtime.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs
	dotnet pack Whisper.net/Whisper.net.csproj -p:Version=$(VERSION) -o ./nupkgs -c $(BUILD_TYPE)

nuget_coreml:
	mkdir -p nupkgs
	nuget pack Whisper.net.Runtime.CoreML.nuspec -Version $(VERSION) -OutputDirectory ./nupkgs

clean:
	rm -rf nupkgs
	rm -rf build
	rm -rf runtimes

android: android_x86 android_x64 android_arm64-v8a

apple: macos ios ios_64 maccatalyst_x64 maccatalyst_arm64 ios_simulator_x64 ios_simulator_arm64 tvos_simulator_x64 tvos_simulator_arm64 tvos lipo

apple_coreml: macos_coreml ios_coreml maccatalyst_x64_coreml maccatalyst_arm64_coreml ios_simulator_coreml tvos_simulator_coreml tvos_coreml lipo_coreml

linux: linux_x64 linux_arm64 linux_arm

linux_x64:
	rm -rf build/linux-x64
	cmake -S . -B build/linux-x64
	cmake --build build/linux-x64 --config $(BUILD_TYPE)
	cp build/linux-x64/whisper.cpp/libwhisper.so ./Whisper.net.Runtime/runtimes/linux-x64/whisper.so

linux_arm64:
	rm -rf build/linux-arm64
	cmake -S . -B build/linux-arm64
	cmake --build build/linux-arm64 --config $(BUILD_TYPE)
	cp build/linux-arm64/whisper.cpp/libwhisper.so ./Whisper.net.Runtime/runtimes/linux-arm64/whisper.so

linux_arm:
	rm -rf build/linux-arm
	cmake -S . -B build/linux-arm
	cmake --build build/linux-arm --config $(BUILD_TYPE)
	cp build/linux-arm/whisper.cpp/libwhisper.so ./Whisper.net.Runtime/runtimes/linux-arm/whisper.so

macos:
	rm -rf build/macos
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=MAC_UNIVERSAL -S . -B build/macos
	cmake --build build/macos
	mkdir -p runtimes/macos
	cp build/macos/whisper.cpp/libwhisper.dylib runtimes/macos/libwhisper.dylib

macos_coreml:
	rm -rf build/macos-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/macos-coreml
	cmake --build build/macos-coreml
	mkdir -p runtimes/macos-coreml
	cp build/macos-coreml/whisper.cpp/libwhisper.dylib runtimes/macos-coreml/libwhisper.dylib
	cp build/macos-coreml/whisper.cpp/libwhisper.coreml.dylib runtimes/macos-coreml/libwhisper.coreml.dylib

ios:
	rm -rf build/ios
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=OS -S . -B build/ios
	cmake --build build/ios
	mkdir -p runtimes/ios
	cp build/ios/whisper.cpp/libwhisper.dylib runtimes/ios/libwhisper.dylib

ios_coreml:
	rm -rf build/ios-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_OSX_SYSROOT="iphoneos" -S . -B build/ios-coreml
	cmake --build build/ios-coreml
	mkdir -p runtimes/ios-coreml
	cp build/ios-coreml/whisper.cpp/libwhisper.coreml.dylib runtimes/ios-coreml/libwhisper.coreml.dylib
	cp build/ios-coreml/whisper.cpp/libwhisper.dylib runtimes/ios-coreml/libwhisper.dylib

ios_64:
	rm -rf build/ios_64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=OS64 -S . -B build/ios_64
	cmake --build build/ios_64
	mkdir -p runtimes/ios_64
	cp build/ios_64/whisper.cpp/libwhisper.dylib runtimes/ios_64/libwhisper.dylib

maccatalyst_x64:
	rm -rf build/maccatalyst_x64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=MAC_CATALYST -S . -B build/maccatalyst_x64
	cmake --build build/maccatalyst_x64
	mkdir -p runtimes/maccatalyst_x64
	cp build/maccatalyst_x64/whisper.cpp/libwhisper.dylib runtimes/maccatalyst_x64/libwhisper.dylib

maccatalyst_arm64:
	rm -rf build/maccatalyst_arm64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=MAC_CATALYST_ARM64 -S . -B build/maccatalyst_arm64
	cmake --build build/maccatalyst_arm64
	mkdir -p runtimes/maccatalyst_arm64
	cp build/maccatalyst_arm64/whisper.cpp/libwhisper.dylib runtimes/maccatalyst_arm64/libwhisper.dylib

maccatalyst_x64_coreml:
	rm -rf build/maccatalyst-x64-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DCMAKE_HOST_SYSTEM_PROCESSOR=x86_64 -DCMAKE_SYSTEM_NAME=Darwin -DCMAKE_OSX_ARCHITECTURES="x86_64" -DCMAKE_CXX_FLAGS="-target x86_64-apple-ios13.1-macabi" -DCMAKE_C_FLAGS="-target x86_64-apple-ios13.1-macabi" -S . -B build/maccatalyst-x64-coreml
	cmake --build build/maccatalyst-x64-coreml
	mkdir -p runtimes/maccatalyst-x64-coreml
	cp build/maccatalyst-x64-coreml/whisper.cpp/libwhisper.coreml.dylib runtimes/maccatalyst-x64-coreml/libwhisper.coreml.dylib
	cp build/maccatalyst-x64-coreml/whisper.cpp/libwhisper.dylib runtimes/maccatalyst-x64-coreml/libwhisper.dylib

maccatalyst_arm64_coreml:
	rm -rf build/maccatalyst-arm64-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_SYSTEM_PROCESSOR=arm -DCMAKE_HOST_SYSTEM_PROCESSOR=arm64 -DCMAKE_SYSTEM_NAME=Darwin -DCMAKE_OSX_ARCHITECTURES="arm64" -DCMAKE_CXX_FLAGS="-target arm64-apple-ios13.1-macabi" -DCMAKE_C_FLAGS="-target arm64-apple-ios13.1-macabi" -S . -B build/maccatalyst-arm64-coreml
	cmake --build build/maccatalyst-arm64-coreml
	mkdir -p runtimes/maccatalyst-arm64-coreml
	cp build/maccatalyst-arm64-coreml/whisper.cpp/libwhisper.coreml.dylib runtimes/maccatalyst-arm64-coreml/libwhisper.coreml.dylib
	cp build/maccatalyst-arm64-coreml/whisper.cpp/libwhisper.dylib runtimes/maccatalyst-arm64-coreml/libwhisper.dylib

ios_simulator_coreml:
	rm -rf build/ios-simulator-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_SYSROOT="iphonesimulator" -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/ios-simulator-coreml
	cmake --build build/ios-simulator-coreml
	mkdir -p runtimes/ios-simulator-coreml
	cp build/ios-simulator-coreml/whisper.cpp/libwhisper.coreml.dylib runtimes/ios-simulator-coreml/libwhisper.coreml.dylib
	cp build/ios-simulator-coreml/whisper.cpp/libwhisper.dylib runtimes/ios-simulator-coreml/libwhisper.dylib

ios_simulator_x64:
	rm -rf build/ios_simulator_x64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=SIMULATOR64 -S . -B build/ios_simulator_x64
	cmake --build build/ios_simulator_x64
	mkdir -p runtimes/ios_simulator_x64
	cp build/ios_simulator_x64/whisper.cpp/libwhisper.dylib runtimes/ios_simulator_x64/libwhisper.dylib

ios_simulator_arm64:
	rm -rf build/ios_simulator_arm64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=SIMULATORARM64 -S . -B build/ios_simulator_arm64
	cmake --build build/ios_simulator_arm64
	mkdir -p runtimes/ios_simulator_arm64
	cp build/ios_simulator_arm64/whisper.cpp/libwhisper.dylib runtimes/ios_simulator_arm64/libwhisper.dylib

tvos_simulator_x64:
	rm -rf build/tvos_simulator_x64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=SIMULATOR_TVOS -S . -B build/tvos_simulator_x64
	cmake --build build/tvos_simulator_x64
	mkdir -p runtimes/tvos_simulator_x64
	cp build/tvos_simulator_x64/whisper.cpp/libwhisper.dylib runtimes/tvos_simulator_x64/libwhisper.dylib

tvos_simulator_arm64:
	rm -rf build/tvos_simulator_arm64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=SIMULATOR_TVOSARM64 -S . -B build/tvos_simulator_arm64
	cmake --build build/tvos_simulator_arm64
	mkdir -p runtimes/tvos_simulator_arm64
	cp build/tvos_simulator_arm64/whisper.cpp/libwhisper.dylib runtimes/tvos_simulator_arm64/libwhisper.dylib

tvos:
	rm -rf build/tvos
	cmake $(CMAKE_PARAMETERS) -DCMAKE_TOOLCHAIN_FILE=ios.toolchain.cmake -DPLATFORM=TVOS -S . -B build/tvos
	cmake --build build/tvos
	mkdir -p runtimes/tvos
	cp build/tvos/whisper.cpp/libwhisper.dylib runtimes/tvos/libwhisper.dylib

tvos_coreml:
	rm -rf build/tvos-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_SYSROOT="appletvos" -DCMAKE_OSX_ARCHITECTURES="arm64" -S . -B build/tvos-coreml
	cmake --build build/tvos-coreml
	mkdir -p runtimes/tvos-coreml
	cp build/tvos-coreml/whisper.cpp/libwhisper.coreml.dylib runtimes/tvos-coreml/libwhisper.coreml.dylib
	cp build/tvos-coreml/whisper.cpp/libwhisper.dylib runtimes/tvos-coreml/libwhisper.dylib

tvos_simulator_coreml:
	rm -rf build/tvos-simulator-coreml
	cmake $(COREML_SUPPORT) -DCMAKE_OSX_SYSROOT="appletvsimulator" -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -S . -B build/tvos-simulator-coreml
	cmake --build build/tvos-simulator-coreml
	mkdir -p runtimes/tvos-simulator-coreml
	cp build/tvos-simulator-coreml/whisper.cpp/libwhisper.coreml.dylib runtimes/tvos-simulator-coreml/libwhisper.coreml.dylib
	cp build/tvos-simulator-coreml/whisper.cpp/libwhisper.dylib runtimes/tvos-simulator-coreml/libwhisper.dylib

lipo_coreml:
	mkdir -p Whisper.net.Runtime.CoreML/tvos-simulator
	cp runtimes/tvos-simulator-coreml/libwhisper.dylib Whisper.net.Runtime.CoreML/tvos-simulator/libwhisper.dylib
	cp runtimes/tvos-simulator-coreml/libwhisper.coreml.dylib Whisper.net.Runtime.CoreML/tvos-simulator/libwhisper.coreml.dylib
	install_name_tool -add_rpath "@executable_path/" Whisper.net.Runtime.CoreML/tvos-simulator/libwhisper.dylib
	mkdir -p Whisper.net.Runtime.CoreML/tvos-device
	cp runtimes/tvos-coreml/libwhisper.dylib Whisper.net.Runtime.CoreML/tvos-device/libwhisper.dylib
	cp runtimes/tvos-coreml/libwhisper.coreml.dylib Whisper.net.Runtime.CoreML/tvos-device/libwhisper.coreml.dylib
	install_name_tool -add_rpath "@executable_path/" Whisper.net.Runtime.CoreML/tvos-device/libwhisper.dylib
	mkdir -p Whisper.net.Runtime.CoreML/ios-simulator
	cp runtimes/ios-simulator-coreml/libwhisper.dylib Whisper.net.Runtime.CoreML/ios-simulator/libwhisper.dylib
	cp runtimes/ios-simulator-coreml/libwhisper.coreml.dylib Whisper.net.Runtime.CoreML/ios-simulator/libwhisper.coreml.dylib
	install_name_tool -add_rpath "@executable_path/" Whisper.net.Runtime.CoreML/ios-simulator/libwhisper.dylib
	mkdir -p Whisper.net.Runtime.CoreML/ios-device
	cp runtimes/ios-coreml/libwhisper.dylib Whisper.net.Runtime.CoreML/ios-device/libwhisper.dylib
	cp runtimes/ios-coreml/libwhisper.coreml.dylib Whisper.net.Runtime.CoreML/ios-device/libwhisper.coreml.dylib
	install_name_tool -add_rpath "@executable_path/" Whisper.net.Runtime.CoreML/ios-device/libwhisper.dylib
	mkdir -p Whisper.net.Runtime.CoreML/macos
	cp runtimes/macos-coreml/libwhisper.dylib Whisper.net.Runtime.CoreML/macos/libwhisper.dylib
	cp runtimes/macos-coreml/libwhisper.coreml.dylib Whisper.net.Runtime.CoreML/macos/libwhisper.coreml.dylib
	install_name_tool -add_rpath "@executable_path/../MonoBundle" Whisper.net.Runtime.CoreML/macos/libwhisper.dylib
	install_name_tool -add_rpath "@executable_path/runtimes/osx-arm64/" Whisper.net.Runtime.CoreML/macos/libwhisper.dylib
	install_name_tool -add_rpath "@executable_path/runtimes/osx-x64/" Whisper.net.Runtime.CoreML/macos/libwhisper.dylib
	mkdir -p Whisper.net.Runtime.CoreML/maccatalyst
	lipo -create runtimes/maccatalyst-x64-coreml/libwhisper.dylib -create runtimes/maccatalyst-arm64-coreml/libwhisper.dylib -output Whisper.net.Runtime.CoreML/maccatalyst/libwhisper.dylib
	lipo -create runtimes/maccatalyst-x64-coreml/libwhisper.coreml.dylib -create runtimes/maccatalyst-arm64-coreml/libwhisper.coreml.dylib -output Whisper.net.Runtime.CoreML/maccatalyst/libwhisper.coreml.dylib
	install_name_tool -add_rpath "@executable_path/../MonoBundle" Whisper.net.Runtime.CoreML/maccatalyst/libwhisper.dylib

lipo:
	mkdir -p Whisper.net.Runtime/tvos-simulator
	lipo -create runtimes/tvos_simulator_arm64/libwhisper.dylib -create runtimes/tvos_simulator_x64/libwhisper.dylib -output Whisper.net.Runtime/tvos-simulator/libwhisper.dylib
	mkdir -p Whisper.net.Runtime/ios-simulator
	lipo -create runtimes/ios_simulator_arm64/libwhisper.dylib -create runtimes/ios_simulator_x64/libwhisper.dylib -output Whisper.net.Runtime/ios-simulator/libwhisper.dylib
	mkdir -p Whisper.net.Runtime/ios-device
	cp runtimes/ios/libwhisper.dylib Whisper.net.Runtime/ios-device/libwhisper.dylib
	mkdir -p Whisper.net.Runtime/maccatalyst
	lipo -create runtimes/maccatalyst_x64/libwhisper.dylib -create runtimes/maccatalyst_arm64/libwhisper.dylib -output Whisper.net.Runtime/maccatalyst/libwhisper.dylib
	mkdir -p Whisper.net.Runtime/tvos-device
	cp runtimes/tvos/libwhisper.dylib Whisper.net.Runtime/tvos-device/libwhisper.dylib
	mkdir -p Whisper.net.Runtime/macos
	cp runtimes/macos/libwhisper.dylib Whisper.net.Runtime/macos/libwhisper.dylib

android_arm64-v8a:
	rm -rf build/android-arm64-v8a
	cmake $(CMAKE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=arm64-v8a -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -S . -B build/android-arm64-v8a
	cmake --build build/android-arm64-v8a
	mkdir -p Whisper.net.Runtime/android-arm64-v8a
	cp build/android-arm64-v8a/whisper.cpp/libwhisper.so Whisper.net.Runtime/android-arm64-v8a/libwhisper.so

android_x86:
	rm -rf build/android-x86
	cmake $(CMAKE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=x86 -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -S . -B build/android-x86
	cmake --build build/android-x86
	mkdir -p Whisper.net.Runtime/android-x86
	cp build/android-x86/whisper.cpp/libwhisper.so Whisper.net.Runtime/android-x86/libwhisper.so

android_x64:
	rm -rf build/android-x86_64
	cmake $(CMAKE_PARAMETERS) -DCMAKE_ANDROID_ARCH_ABI=x86_64 -DCMAKE_SYSTEM_NAME=Android -DCMAKE_ANDROID_API=21 -DCMAKE_ANDROID_NDK=$(NDK) -S . -B build/android-x86_64
	cmake --build build/android-x86_64
	mkdir -p Whisper.net.Runtime/android-x86_64
	cp build/android-x86_64/whisper.cpp/libwhisper.so Whisper.net.Runtime/android-x86_64/libwhisper.so

xcframework:
	mkdir -p output/lib
	xcrun xcodebuild -create-xcframework -library Whisper.net.Runtime/ios-device/libwhisper.dylib -library Whisper.net.Runtime/ios-simulator/libwhisper.dylib -library Whisper.net.Runtime/tvos-device/libwhisper.dylib -library Whisper.net.Runtime/tvos-simulator/libwhisper.dylib -library Whisper.net.Runtime/macos/libwhisper.dylib -library Whisper.net.Runtime/maccatalyst/libwhisper.dylib -output output/lib/whisper.xcframework
