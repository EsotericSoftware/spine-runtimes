#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
# shellcheck source=common.sh
source "$dir/common.sh"
install_pause_on_exit

pushd "$dir" > /dev/null

if [ ! -d ../godot ]; then
	echo "No Godot clone found. Run ./setup.sh <Godot branch or tag> <dev> first."
	exit 1
fi

# Detect system
cpus=2
if [[ "$OSTYPE" == "darwin"* ]]; then
	cpus=$(sysctl -n hw.logicalcpu)
	arch="arm64"
	if [ `uname -m` == "x86_64" ]; then
		arch="x86_64"
	fi
else
	cpus=$(grep -c ^processor /proc/cpuinfo)
fi

echo "Generating compile_commands.json..."
pushd ../godot

# Generate essential header files first
echo "Generating required header files..."
scons -j$cpus custom_modules="../spine_godot" opengl3=yes arch=$arch \
    core/version_generated.gen.h \
    core/disabled_classes.gen.h \
    core/object/gdvirtual.gen.inc

# Now generate compile_commands.json
# The 'compiledb' target specifically generates only the compile_commands.json
scons compiledb=yes custom_modules="../spine_godot" opengl3=yes arch=$arch compiledb

# Copy the compile_commands.json to the parent directory for easy IDE access
if [ -f compile_commands.json ]; then
	cp compile_commands.json ..
	echo "compile_commands.json generated successfully and copied to spine-godot/"
else
	echo "Failed to generate compile_commands.json"
fi

popd
popd > /dev/null