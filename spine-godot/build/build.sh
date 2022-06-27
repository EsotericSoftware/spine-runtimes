#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null

if [ ! "$#" -eq 0 ] && [ ! "$#" -eq 2 ]; then
	echo "Usage: ./build.sh <Godot branch or tag> <dev>"
	echo
	echo "e.g.:"
	echo "       ./build.sh 3.4.4-stable true"
	echo "       ./build.sh master master"
	echo
	echo "If no arguments are given"
	read version
	exit 1
fi

if [ "$#" -eq 0 ] && [ ! -d ../godot ]; then
	echo "No Godot clone found. Run ./build.sh <Godot branch or tag> <dev> first."
	exit 1
fi

branch=${1%/}
if [ "${2}" = "true" ]; then
target="target=debug"
else
target=""
fi

pushd ..
echo `pwd`
if [ ! -z "$branch" ]; then
	rm -rf godot
	git clone --depth 1 https://github.com/godotengine/godot.git -b $branch
	if [ "${2}" = "true" ]; then
		cp -r .idea godot
		cp build/custom.py godot
		rm -rf example/.import
		rm -rf example/.godot
	fi
fi
cp -r ../spine-cpp/spine-cpp spine_godot
popd


pushd ../godot
scons $target arch=x86_64 compiledb=yes custom_modules="../spine_godot" -j16
if [ `uname` == 'Darwin' ]; then	
	scons $target arch=arm64 compiledb=yes custom_modules="../spine_godot" -j16
	lipo -create bin/godot.osx.tools.x86_64 bin/godot.osx.tools.arm64 -output bin/godot.osx.tools.universal
	strip -S -x bin/godot.osx.tools.universal
fi
popd

popd > /dev/null