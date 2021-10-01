#!/bin/bash
set -e
rm -rf godot
cp -r ../spine-cpp/spine-cpp spine-godot
git clone --depth 1 https://github.com/godotengine/godot.git -b 3.3.4-stable
cp custom.py godot
ln -s $(pwd)/spine_godot godot/modules/spine_godot
pushd godot
scons target=debug --jobs=$(sysctl -n hw.logicalcpu)
popd