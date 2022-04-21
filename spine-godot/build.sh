#/bin/sh
set -e
pushd godot
scons compiledb=yes custom_modules="../spine_godot" -j16
popd