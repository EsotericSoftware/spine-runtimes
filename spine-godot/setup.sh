#!/bin/bash
set -e
rm -rf godot
git clone --depth 1 https://github.com/godotengine/godot.git -b 3.4.4-stable
cp -r .idea godot
cp custom.py godot
cp -r ../spine-cpp/spine-cpp spine_godot
./build.sh