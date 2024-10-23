#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir" > /dev/null

if [ $# -lt 2 ] || [ $# -gt 4 ]; then
	echo "Usage: ./setup.sh <Godot branch or tag> <dev:true|false> <mono:true|false>? <godot-repository>?"
	echo
	echo "e.g.:"
	echo "       ./setup.sh 4.2.1-stable true"
	echo "       ./setup.sh master false true"
	echo "       ./setup.sh master false false https://github.com/my-github-username/godot.git"
	echo
	echo "Note: the 'mono' parameter only works for Godot 4.x+!"

	exit 1
fi

branch=${1%/}
dev=${2%/}
mono=false
repo=https://github.com/godotengine/godot.git
version=$(echo $branch | cut -d. -f1-2)
echo $version > version.txt

if [[ $# -eq 3 && "$branch" != 3* ]]; then
	mono=${3%/}
fi

if [ "$dev" != "true" ] && [ "$dev" != "false" ]; then
	echo "Invalid value for the 'dev' argument. It should be either 'true' or 'false'."
	exit 1
fi

if [ "$mono" != "true" ] && [ "$mono" != "false" ]; then
	echo "Invalid value for the 'mono' argument. It should be either 'true' or 'false'."
	exit 1
fi

if [ $# -eq 4 ]; then
    repo=${4%/}
fi

pushd ..
rm -rf godot
git clone --depth 1 $repo -b $branch
if [ $dev = "true" ]; then
	cp build/custom.py godot
	if [ "$mono" = "true" ]; then
		echo "" >> godot/custom.py
    	echo "module_mono_enabled=\"yes\"" >> godot/custom.py
	fi
	cp ../formatters/.clang-format .
	rm -rf example/.import
	rm -rf example/.godot

	if [ `uname` == 'Darwin' ] && [ ! -d "$HOME/VulkanSDK" ]; then
		./build/install-macos-vulkan-sdk.sh
	fi
fi
cp -r ../spine-cpp/spine-cpp spine_godot
popd

popd > /dev/null