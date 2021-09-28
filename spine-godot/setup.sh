#!/bin/bash
set -e
rm -rf godot
cp -r ../spine-cpp/spine-cpp spine-godot
git clone --depth 1 https://github.com/godotengine/godot.git -b 3.3.3-stable
ln -s $(pwd)/spine_godot godot/modules/spine_godot
pushd godot
scons --jobs=$(sysctl -n hw.logicalcpu)
popd