#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
# shellcheck source=common.sh
source "$dir/common.sh"
install_pause_on_exit

pushd "$dir" > /dev/null

if [ ! -d ../godot-cpp ]; then
    echo "No godot-cpp clone found. Run ./setup-extension.sh <Godot branch or tag> <dev> first."
    exit 1
fi

options=""
dev="false"
raw_platform=${1%/}  # Store the raw platform name before adding platform=
platform="platform=$raw_platform"  # Add platform= prefix
arch=$2

if [ -f "../godot-cpp/dev" ]; then
    dev="true"
    echo "DEV build"
fi

if [ $dev == "true" ]; then
    options="$options dev_build=true"
fi

if [ -z $raw_platform ]; then
    echo "Platform: current"
    platform=""
else
    echo "Platform: $raw_platform"
fi

if [ ! -z "$arch" ]; then
    echo "Architecture: $arch"
    if [ "$raw_platform" == "linux" ] || [ "$raw_platform" == "android" ]; then
        options="$options arch=$arch"
    fi
fi

cpus=2
if [ "$OSTYPE" == "msys" ]; then
    os="windows"
    cpus=$NUMBER_OF_PROCESSORS
elif [[ "$OSTYPE" == "darwin"* ]]; then
    os="macos"
    cpus=$(sysctl -n hw.logicalcpu)
    if [ `uname -m` == "arm64" ]; then
        echo "Would do Apple Silicon specific setup"
    fi
else
    os="linux"
    cpus=$(grep -c ^processor /proc/cpuinfo)
fi

echo "CPUS: $cpus"

pushd ..

if [ "$raw_platform" == "web" ]; then
    BINDIR="example-v4-extension/bin/web"
    mkdir -p $BINDIR

    # Build threaded versions
    echo "Building web with threads..."
    scons -j $cpus $options $platform target=template_debug
    scons -j $cpus $options $platform target=template_release

    # Build non-threaded versions
    echo "Building web without threads..."
    scons -j $cpus $options $platform target=template_debug threads=no
    scons -j $cpus $options $platform target=template_release threads=no
else
    # Normal build process for other platforms
    if [ "$raw_platform" != "android" ] && [ "$raw_platform" != "ios" ] && [ "$raw_platform" != "web" ]; then
        scons -j $cpus $options $platform target=editor
    fi
    scons -j $cpus $options $platform target=template_debug
    scons -j $cpus $options $platform target=template_release
fi

popd

popd