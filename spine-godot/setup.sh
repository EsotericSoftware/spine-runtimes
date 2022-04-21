#!/bin/bash
set -e

if [ "$#" -eq 0 ]; then
	echo "Usage: ./setup.sh <godot_branch_or_tag>"
	echo
	echo "e.g.:"
	echo "       ./setup.sh 3.4.4-stable"
	echo "       ./setup.sh master"
	read version
	exit 1
fi

branch=${1%/}

rm -rf godot
git clone --depth 1 https://github.com/godotengine/godot.git -b $branch
cp -r .idea godot
cp custom.py godot
cp -r ../spine-cpp/spine-cpp spine_godot
rm -rf example/.import
rm -rf example/.godot
./build.sh