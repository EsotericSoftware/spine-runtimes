#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir" > /dev/null

if [ ! "$#" -eq 1 ]; then
	echo "Usage: ./build.sh <target>"
	echo
	echo "e.g.:"
	echo "       ./build.sh release_debug"
	echo "       ./build.sh debug"
	echo
	exit 1
fi

if [ ! -d ../godot ]; then
	echo "No Godot clone found. Run ./setup.sh <Godot branch or tag> <dev> first."
	exit 1
fi

target="target=${1%/}"
dev="false"
if [ -f "../godot/custom.py" ]; then
	dev="true"
fi

cpus=2
if [ "$OSTYPE" = "msys" ]; then
	cpus=$NUMBER_OF_PROCESSORS
elif [[ "$OSTYPE" = "darwin"* ]]; then
	cpus=$(sysctl -n hw.logicalcpu)
else
	cpus=$(grep -c ^processor /proc/cpuinfo)
fi

echo "CPUS: $cpus"

pushd ../godot
if [ `uname` == 'Darwin' ] && [ $dev = "false" ]; then
	scons $target arch=x86_64 compiledb=yes custom_modules="../spine_godot" --jobs=$cpus
	scons $target arch=arm64 compiledb=yes custom_modules="../spine_godot" --jobs=$cpus

	pushd bin
	cp -r ../misc/dist/osx_tools.app .
	mv osx_tools.app Godot.app
	mkdir -p Godot.app/Contents/MacOS
	if [ "$target" = "debug" ]; then
		lipo -create godot.osx.tools.x86_64 godot.osx.tools.arm64 -output godot.osx.tools.universal
		strip -S -x godot.osx.tools.universal
		cp godot.osx.tools.universal Godot.app/Contents/MacOS/Godot
	else
		lipo -create godot.osx.opt.tools.x86_64 godot.osx.opt.tools.arm64 -output godot.osx.opt.tools.universal
		strip -S -x godot.osx.opt.tools.universal
		cp godot.osx.opt.tools.universal Godot.app/Contents/MacOS/Godot
	fi
	chmod +x Godot.app/Contents/MacOS/Godot
	popd
else
	if [ "$OSTYPE" = "msys" ] || [ "$RUNNER_OS" = "Windows" ]; then
		target="$target vsproj=yes livepp=$LIVEPP"
	fi
	scons $target compiledb=yes custom_modules="../spine_godot" --jobs=$cpus
	cp compile_commands.json ../build
	if [ -f "bin/godot.x11.opt.tools.64" ]; then
		strip bin/godot.x11.opt.tools.64
		chmod a+x bin/godot.x11.opt.tools.64
	fi
fi
popd

popd > /dev/null