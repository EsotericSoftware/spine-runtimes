#!/bin/bash
set -e
set -x

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir" > /dev/null

if [ "$#" -lt 1 ]; then
	echo "Usage: ./build-templates.sh <platform> <mono:true|false>?"
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
mono=false
version=$(cat version.txt)
major=$(echo $version | cut -d. -f1)
minor=$(echo $version | cut -d. -f2)

if [[ $# -eq 2 ]]; then
	mono=${2%/}
	if [ "$platform" != "windows" ] && [ "$platform" != "linux" ] && [ "$platform" != "macos" ]; then
		echo "C# is only supported for Windows, Linux, and macOS"
		exit 1
	fi
	echo "Building Godot template with C# support"
else
	echo "Building Godot template without C# support"
fi

mono_module=""
mono_extension=""
if [ $mono == "true" ]; then
	mono_module="module_mono_enabled=yes"
	mono_extension=".mono"
	echo "Building Godot with C# support"
else
	echo "Building Godot without C# support"
fi

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
	scons platform=windows tools=no target=template_release custom_modules="../spine_godot" $mono_module --jobs=$cpus
	scons platform=windows tools=no target=template_debug custom_modules="../spine_godot" $mono_module --jobs=$cpus
	cp bin/godot.windows.template_release.x86_64$mono_extension.exe bin/windows_release_x86_64.exe
	cp bin/godot.windows.template_debug.x86_64$mono_extension.exe bin/windows_debug_x86_64.exe

elif [ "$platform" = "macos" ]; then
	# --- macOS ---
	# generates macos.zip

	scons platform=macos tools=no target=template_release arch=x86_64 custom_modules="../spine_godot" $mono_module --jobs=$cpus
	scons platform=macos tools=no target=template_debug arch=x86_64 custom_modules="../spine_godot" $mono_module --jobs=$cpus
	scons platform=macos tools=no target=template_release arch=arm64 custom_modules="../spine_godot" $mono_module --jobs=$cpus
	scons platform=macos tools=no target=template_debug arch=arm64 custom_modules="../spine_godot" $mono_module --jobs=$cpus
	lipo -create "bin/godot.macos.template_release.x86_64$mono_extension" "bin/godot.macos.template_release.arm64$mono_extension" -output bin/godot.macos.universal
	lipo -create "bin/godot.macos.template_debug.x86_64$mono_extension" "bin/godot.macos.template_debug.arm64$mono_extension" -output bin/godot.macos.debug.universal
	strip -S -x bin/godot.macos.universal

	pushd bin
	cp -r ../misc/dist/macos_template.app .
	mkdir -p macos_template.app/Contents/MacOS
	cp godot.macos.universal macos_template.app/Contents/MacOS/godot_macos_release.universal
	cp godot.macos.debug.universal macos_template.app/Contents/MacOS/godot_macos_debug.universal
	chmod +x macos_template.app/Contents/MacOS/godot_macos*
	rm -rf macos.zip
	zip -q -9 -r macos.zip macos_template.app
	popd
elif [ "$platform" = "linux" ]; then
	# --- Linux ---
	# generates linux_x11_64_release, linux_x11_64_debug
	scons platform=linuxbsd tools=no target=template_release bits=64 custom_modules="../spine_godot" $mono_module --jobs=$cpus
	scons platform=linuxbsd tools=no target=template_debug bits=64 custom_modules="../spine_godot" $mono_module --jobs=$cpus
	strip bin/godot.linuxbsd.template_release.x86_64$mono_extension
	strip bin/godot.linuxbsd.template_debug.x86_64$mono_extension
	chmod a+x bin/godot.linuxbsd.template_release.x86_64$mono_extension
	chmod a+x bin/godot.linuxbsd.template_debug.x86_64$mono_extension
	cp bin/godot.linuxbsd.template_release.x86_64$mono_extension bin/linux_release.x86_64
	cp bin/godot.linuxbsd.template_debug.x86_64$mono_extension bin/linux_debug.x86_64
elif [ "$platform" = "ios" ]; then
	# --- iOS --
	# generates ios.zip

	scons p=ios tools=no target=template_release arch=arm64 custom_modules="../spine_godot" --jobs=$cpus
	scons p=ios tools=no target=template_debug arch=arm64 custom_modules="../spine_godot" --jobs=$cpus
	scons p=ios tools=no target=template_release arch=arm64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	scons p=ios tools=no target=template_release arch=x86_64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	scons p=ios tools=no target=template_debug arch=arm64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	scons p=ios tools=no target=template_debug arch=x86_64 ios_simulator=yes custom_modules="../spine_godot" --jobs=$cpus
	lipo -create bin/libgodot.ios.template_release.arm64.simulator.a bin/libgodot.ios.template_release.x86_64.simulator.a -output bin/libgodot.ios.template_release.simulator.a
	lipo -create bin/libgodot.ios.template_debug.arm64.simulator.a bin/libgodot.ios.template_debug.x86_64.simulator.a -output bin/libgodot.ios.template_debug.simulator.a
	strip -S -x bin/libgodot.ios.template_release.arm64.a
	strip -S -x bin/libgodot.ios.template_release.simulator.a

	pushd bin
	cp -r ../misc/dist/ios_xcode .
	cp libgodot.ios.template_release.arm64.a ios_xcode/libgodot.ios.release.xcframework/ios-arm64/libgodot.a
	cp libgodot.ios.template_release.simulator.a ios_xcode/libgodot.ios.release.xcframework/ios-arm64_x86_64-simulator/libgodot.a
	cp libgodot.ios.template_debug.arm64.a ios_xcode/libgodot.ios.debug.xcframework/ios-arm64/libgodot.a
	cp libgodot.ios.template_debug.simulator.a ios_xcode/libgodot.ios.debug.xcframework/ios-arm64_x86_64-simulator/libgodot.a
	cp -r ~/VulkanSDK/1.3.250.1/MoltenVK/MoltenVK.xcframework ios_xcode/
  	rm -rf ios_xcode/MoltenVK.xcframework/{macos,tvos}*
	rm -rf ios.zip
	pushd ios_xcode
	zip -q -9 -r ../ios.zip *
	popd
	popd
elif [ "$platform" = "web" ]; then
	# --- WEB ---
	# generates webassembly_debug.zip, webassembly_release.zip
	scons platform=web tools=no target=template_release custom_modules="../spine_godot" --jobs=$cpus
	scons platform=web tools=no target=template_debug custom_modules="../spine_godot" --jobs=$cpus
	mv bin/godot.web.template_release.wasm32.zip bin/web_release.zip
	mv bin/godot.web.template_debug.wasm32.zip bin/web_debug.zip

	scons platform=web tools=no threads=no target=template_release custom_modules="../spine_godot" --jobs=$cpus
	scons platform=web tools=no threads=no target=template_debug custom_modules="../spine_godot" --jobs=$cpus

	if [ -f bin/godot.web.template_release.wasm32.nothreads.zip ]; then
		mv bin/godot.web.template_release.wasm32.nothreads.zip bin/web_nothreads_release.zip
	else
		mv bin/godot.web.template_release.wasm32.zip bin/web_nothreads_release.zip
	fi

	if [ -f bin/godot.web.template_debug.wasm32.nothreads.zip ]; then
		mv bin/godot.web.template_debug.wasm32.nothreads.zip bin/web_nothreads_debug.zip
	else
		mv bin/godot.web.template_debug.wasm32.zip bin/web_nothreads_debug.zip
	fi

	scons platform=web tools=no dlink_enabled=yes target=template_release custom_modules="../spine_godot" --jobs=$cpus
	scons platform=web tools=no dlink_enabled=yes target=template_debug custom_modules="../spine_godot" --jobs=$cpus
	mv bin/godot.web.template_release.wasm32.dlink.zip bin/web_dlink_release.zip
	mv bin/godot.web.template_debug.wasm32.dlink.zip bin/web_dlink_debug.zip

	scons platform=web tools=no threads=no dlink_enabled=yes target=template_release custom_modules="../spine_godot" --jobs=$cpus
	scons platform=web tools=no threads=no dlink_enabled=yes target=template_debug custom_modules="../spine_godot" --jobs=$cpus

	if [ -f bin/godot.web.template_release.wasm32.nothreads.dlink.zip ]; then
		mv bin/godot.web.template_release.wasm32.nothreads.dlink.zip bin/web_dlink_nothreads_release.zip
	else
		mv bin/godot.web.template_release.wasm32.dlink.zip bin/web_dlink_nothreads_release.zip
	fi

	if [ -f bin/godot.web.template_debug.wasm32.nothreads.dlink.zip ]; then
		mv bin/godot.web.template_debug.wasm32.nothreads.dlink.zip bin/web_dlink_nothreads_debug.zip
	else
		mv bin/godot.web.template_debug.wasm32.dlink.zip bin/web_dlink_nothreads_debug.zip
	fi
elif [ "$platform" = "android" ]; then
	# --- ANROID ---
	# generates android_release.apk, android_debug.apk, android_source.zip
	if [[ $major -lt 4 || ($major -eq 4 && $minor -lt 3) ]]; then
		# Godot < 4.3 generates APKs via Gradle invocation.
		scons platform=android target=template_release arch=arm32 custom_modules="../spine_godot" --jobs=$cpus
		scons platform=android target=template_debug arch=arm32 custom_modules="../spine_godot" --jobs=$cpus
		scons platform=android target=template_release arch=arm64 custom_modules="../spine_godot" --jobs=$cpus
		scons platform=android target=template_debug arch=arm64 custom_modules="../spine_godot" --jobs=$cpus

		pushd platform/android/java
			chmod a+x gradlew
			./gradlew generateGodotTemplates
		popd
	else
		scons platform=android target=template_release arch=arm32 custom_modules="../spine_godot" --jobs=$cpus
		scons platform=android target=template_release arch=arm64 custom_modules="../spine_godot" generate_apk=yes --jobs=$cpus
		scons platform=android target=template_debug arch=arm32 custom_modules="../spine_godot" --jobs=$cpus
		scons platform=android target=template_debug arch=arm64 custom_modules="../spine_godot" generate_apk=yes --jobs=$cpus
	fi
else
	echo "Unknown platform: $platform"
	exit 1
fi
popd
