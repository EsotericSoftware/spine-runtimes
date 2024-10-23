#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir" > /dev/null

if [ $# -lt 2 ] || [ $# -gt 3 ]; then
    echo "Usage: ./setup-extension.sh <Godot version> <dev:true|false> <mono:true|false>?"
    echo
    echo "e.g.:"
    echo "       ./setup-extension.sh 4.2.2-stable true"

    exit 1
fi

godot_branch=${1%/}
dev=${2%/}
mono=false
godot_cpp_repo=https://github.com/godotengine/godot-cpp.git
godot_repo=https://github.com/godotengine/godot.git

if [[ $# -eq 3 ]]; then
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

godot_cpp_branch=$(echo $godot_branch | cut -d. -f1-2)

cpus=2
if [ "$OSTYPE" == "msys" ]; then
	cpus=$NUMBER_OF_PROCESSORS
elif [[ "$OSTYPE" == "darwin"* ]]; then
	cpus=$(sysctl -n hw.logicalcpu)
else
	cpus=$(grep -c ^processor /proc/cpuinfo)
fi

echo "godot-cpp branch: $godot_cpp_branch"
echo "godot branch: $godot_branch"
echo "dev: $dev"
echo "mono: $mono"
echo "cpus: $cpus"

pushd ..

rm -rf godot-cpp
git clone $godot_cpp_repo
pushd godot-cpp
git checkout $godot_cpp_branch
popd

rm -rf godot
git clone $godot_repo
pushd godot
git checkout $godot_branch
scons target=editor dev_build=true optimize=debug --jobs=$cpus
popd

cp -r ../spine-cpp/spine-cpp spine_godot

popd
popd > /dev/null