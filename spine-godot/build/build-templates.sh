#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null

if [ "$#" -eq 0 ] && [ ! -d ../godot ]; then
	echo "No Godot clone found. Run ./build.sh <Godot branch or tag> <dev> first."
	exit 1
fi

pushd ../godot
if [ `uname` == "Darwin" ]; then
	scons platform=osx tools=no target=release arch=x86_64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons platform=osx tools=no target=release_debug arch=x86_64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons platform=osx tools=no target=release arch=arm64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
	scons platform=osx tools=no target=release_debug arch=arm64 custom_modules="../spine_godot" --jobs=$(sysctl -n hw.logicalcpu)
fi
popd
