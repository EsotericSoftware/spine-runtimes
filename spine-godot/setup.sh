#!/bin/bash
set -e
rm -rf godot
git clone --depth 1 https://github.com/godotengine/godot.git -b 3.4.4-stable
cp -r .idea godot
cp custom.py godot
cp -r ../spine-cpp/spine-cpp spine_godot
pushd godot
scons -Q compiledb custom_modules="../spine_godot"
scons target=debug custom_modules="../spine_godot" --jobs=8
popd