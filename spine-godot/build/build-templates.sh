#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir" > /dev/null

if [ ! "$#" -eq 1 ]; then
	echo "Usage: ./build-templates.sh <platform>"
	echo
	echo "e.g.:"
	echo "       ./build-templates.sh windows"
	echo "       ./build-templates.sh linux"
	echo "       ./build-templates.sh macos"
	echo "       ./build-templates.sh ios"
	echo "       ./build-templates.sh android"
	echo "       ./build-templates.sh web"
	echo	
	exit 1
fi

if [ ! -d ../godot ]; then
	echo "No Godot clone found. Run ./setup.sh <Godot branch or tag> <dev> first."
	exit 1
fi

platform=${1%/}

cpus=2
if [ "$OSTYPE" = "msys" ]; then
	cpus=$NUMBER_OF_PROCESSORS
elif [[ "$OSTYPE" = "darwin"* ]]; then
	cpus=$(sysctl -n hw.logicalcpu)
else
	cpus=$(grep -c ^processor /proc/cpuinfo)
fi

pushd ../godot
if [ "$platform" = "windows" ]; then
	# --- Windows ---
	#generates windows_64_debug.exe and windows_64_release.exe
	scons platform=windows tools=no target=release custom_modules="../spine_godot" --jobs=$cpus
	scons platform=windows tools=no target=release_debug custom_modules="../spine_godot" --jobs=$cpus
	cp bin/godot.windows.opt.64.exe bin/windows_64_release.exe
	cp bin/godot.windows.opt.debug.64.exe bin/windows_64_debug.exe

elif [ "$platform" = "macos" ]; then
	# --- macOS ---
	# generates osx.zip

	scons platform=osx tools=no target=release arch=x86_64 custom_modules="../spine_godot" --jobs=$cpus
	scons platform=osx tools=no target=release_debug arch=x86_64 custom_modules="../spine_godot" --jobs=$cpus
	scons platform=osx tools=no target=release arch=arm64 custom_modules="../spine_godot" --jobs=$cpus
	scons platform=osx tools=no target=release_debug arch=arm64 custom_modules="../spine_godot" --jobs=$cpus
	lipo -create bin/godot.osx.opt.x86_64 bin/godot.osx.opt.arm64 -output bin/godot.osx.opt.universal
	lipo -create bin/godot.osx.opt.debug.x86_64 bin/godot.osx.opt.debug.arm64 -output bin/godot.osx.opt.debug.universal
	strip -S -x bin/godot.osx.opt.universal

	pushd bin
	cp -r ../misc/dist/osx_template.app .
	mkdir -p osx_template.app/Contents/MacOS
	cp godot.osx.opt.universal osx_template.app/Contents/MacOS/godot_osx_release.64
	cp godot.osx.opt.debug.universal osx_template.app/Contents/MacOS/godot_osx_debug.64
	chmod +x osx_template.app/Contents/MacOS/godot_osx*		
	rm -rf osx.zip
	zip -q -9 -r osx.zip osx_template.app
	popd
elif [ "$platform" = "ios" ]; then
	# --- iOS --
	# generates iphone.zip

	scons p=iphone tools=no target=release arch=arm64 custom_modules="../spine_godot" --jobs=$cpus
	scons p=iphone tools=no target=release_debug arch=arm64 custom_modules="../spine_godot" --jobs=$cpus
	scons p=iphone tools=no target=release arch=arm64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	scons p=iphone tools=no target=release arch=x86_64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	scons p=iphone tools=no target=release_debug arch=arm64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	scons p=iphone tools=no target=release_debug arch=x86_64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	lipo -create bin/libgodot.iphone.opt.arm64.simulator.a bin/libgodot.iphone.opt.x86_64.simulator.a -output bin/libgodot.iphone.opt.simulator.a
	lipo -create bin/libgodot.iphone.opt.debug.arm64.simulator.a bin/libgodot.iphone.opt.debug.x86_64.simulator.a -output bin/libgodot.iphone.opt.debug.simulator.a
	strip -S -x bin/libgodot.iphone.opt.arm64.a
	strip -S -x bin/libgodot.iphone.opt.simulator.a		

	pushd bin
	cp -r ../misc/dist/ios_xcode .		
	cp libgodot.iphone.opt.arm64.a ios_xcode/libgodot.iphone.release.xcframework/ios-arm64/libgodot.a
	cp libgodot.iphone.opt.simulator.a ios_xcode/libgodot.iphone.release.xcframework/ios-arm64_x86_64-simulator/libgodot.a	
	cp libgodot.iphone.opt.debug.arm64.a ios_xcode/libgodot.iphone.debug.xcframework/ios-arm64/libgodot.a
	cp libgodot.iphone.opt.debug.simulator.a ios_xcode/libgodot.iphone.debug.xcframework/ios-arm64_x86_64-simulator/libgodot.a	
	rm -rf iphone.zip
	pushd ios_xcode
	zip -q -9 -r ../iphone.zip *
	popd
	popd	
elif [ "$platform" = "web" ]; then
	# --- WEB ---
	# generates webassembly_debug.zip, webassembly_release.zip
	scons platform=javascript tools=no target=release custom_modules="../spine_godot" --jobs=$cpus
	scons platform=javascript tools=no target=release_debug custom_modules="../spine_godot" --jobs=$cpus
	mv bin/godot.javascript.opt.zip bin/webassembly_release.zip
	mv bin/godot.javascript.opt.debug.zip bin/webassembly_debug.zip
elif [ "$platform" = "android" ]; then
	# --- ANROID ---
	# generates android_release.apk, android_debug.apk, android_source.zip
	scons platform=android target=release android_arch=armv7 custom_modules="../spine_godot" --jobs=$cpus
	scons platform=android target=release_debug android_arch=armv7 custom_modules="../spine_godot" --jobs=$cpus
	scons platform=android target=release android_arch=arm64v8 custom_modules="../spine_godot" --jobs=$cpus
	scons platform=android target=release_debug android_arch=arm64v8 custom_modules="../spine_godot" --jobs=$cpus

	pushd platform/android/java
		chmod a+x gradlew
		./gradlew generateGodotTemplates
	popd
elif [ "$platform" = "linux" ]; then
	# --- Linix ---
	# generates linux_x11_64_release, linux_x11_64_debug
	scons platform=x11 tools=no target=release bits=64 custom_modules="../spine_godot" --jobs=$cpus
	scons platform=x11 tools=no target=release_debug bits=64 custom_modules="../spine_godot" --jobs=$cpus
	strip bin/godot.x11.opt.64	
	strip bin/godot.x11.opt.debug.64
	chmod a+x bin/godot.x11.opt.64
	chmod a+x bin/godot.x11.opt.debug.64
	cp bin/godot.x11.opt.64 bin/linux_x11_64_release
	cp bin/godot.x11.opt.debug.64 bin/linux_x11_64_debug
else
	echo "Unknown platform: $platform"
	exit 1
fi
popd
