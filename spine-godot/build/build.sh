#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null

if [ ! "$#" -eq 0 ] && [ ! "$#" -eq 2 ]; then
	echo "Usage: ./build.sh <target>"
	echo
	echo "e.g.:"
	echo "       ./build.sh release_debug"
	echo "       ./build.sh debug"
	echo	
	read version
	exit 1
fi

if [ "$#" -eq 0 ]; then
	echo "Please specify a target, either 'debug' or 'release_debug'"
	exit 1
fi

if [ ! -d ../godot ]; then
	echo "No Godot clone found. Run ./setup.sh <Godot branch or tag> <dev> first."
	exit 1
fi

branch=${1%/}

if [ "${2}" = "true" ]; then
	target="target=debug"
else
	target=""
fi


pushd ../godot
scons $target arch=x86_64 compiledb=yes custom_modules="../spine_godot" -j16
if [ `uname` == 'Darwin' ]; then	
	scons $target arch=arm64 compiledb=yes custom_modules="../spine_godot" -j16
	lipo -create bin/godot.osx.tools.x86_64 bin/godot.osx.tools.arm64 -output bin/godot.osx.tools.universal
	strip -S -x bin/godot.osx.tools.universal
fi
popd

popd > /dev/null