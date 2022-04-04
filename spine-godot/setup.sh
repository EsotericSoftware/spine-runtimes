#!/bin/bash
set -e
cp -r ../spine-cpp/spine-cpp godot/modules/spine_godot
git clone --depth 1 https://github.com/godotengine/godot.git -b 3.4.4-stable godot-copy
rm -rf godot-copy/.git
cp -r godot-copy/* godot
cp -r .idea godot
pushd godot
scons target=debug --jobs=$(sysctl -n hw.logicalcpu)
popd