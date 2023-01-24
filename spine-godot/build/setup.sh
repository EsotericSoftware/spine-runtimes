#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null

if [ ! "$#" -eq 2 ]; then
	echo "Usage: ./setup.sh <Godot branch or tag> <dev:true|false>"
	echo
	echo "e.g.:"
	echo "       ./setup.sh 3.4.4-stable true"
	echo "       ./setup.sh master false"
	echo		
	exit 1
fi

branch=${1%/}
dev=${2%/}

pushd ..
rm -rf godot
git clone --depth 1 https://github.com/godotengine/godot.git -b $branch
if [ $dev = "true" ]; then
	cp -r .idea godot
	cp build/custom.py godot
	cp ../formatters/.clang-format .
	rm -rf example/.import
	rm -rf example/.godot

	if [ "$OSTYPE" = "msys" ]; then
		pushd godot
		if [[ $branch == 3* ]]; then
			echo "Applying V3 Live++ patch"
			git apply ../build/livepp.patch
		else
			echo "Applying V4 Live++ patch"
			git apply ../build/livepp-v4.patch
		fi
		popd
	fi

	if [ `uname` == 'Darwin' ] && [ ! -d "$HOME/VulkanSDK" ]; then
		./build/install-macos-vulkan-sdk.sh
	fi
fi
cp -r ../spine-cpp/spine-cpp spine_godot
popd

popd > /dev/null