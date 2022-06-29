#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null

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

pushd ../godot
if [ "$platform" = "macos" ]; then
	# --- macOS ---
	# generates osx.zip

	scons platform=osx tools=no target=release arch=x86_64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons platform=osx tools=no target=release_debug arch=x86_64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons platform=osx tools=no target=release arch=arm64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons platform=osx tools=no target=release_debug arch=arm64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
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

	scons p=iphone tools=no target=release arch=arm64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons p=iphone tools=no target=release_debug arch=arm64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons p=iphone tools=no target=release arch=arm64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons p=iphone tools=no target=release arch=x86_64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons p=iphone tools=no target=release_debug arch=arm64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons p=iphone tools=no target=release_debug arch=x86_64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
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
else
	echo "Unknown platform: $platform"
	exit 1
fi
popd
