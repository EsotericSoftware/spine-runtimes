#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
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

if [ "$raw_platform" == "ios" ]; then
    BINDIR="example-v4-extension/bin/ios"
    mkdir -p $BINDIR

    # Step 1: Build simulator binaries
    echo "Building for iOS simulator..."
    scons -j $cpus $options $platform target=template_debug arch=universal ios_simulator=yes
    mv $BINDIR/ios.framework/libspine_godot.ios.template_debug $BINDIR/libspine_godot.ios.template_debug.simulator.a

    scons -j $cpus $options $platform target=template_release arch=universal ios_simulator=yes
    mv $BINDIR/ios.framework/libspine_godot.ios.template_release $BINDIR/libspine_godot.ios.template_release.simulator.a

    # Step 2: Build device binaries
    echo "Building for iOS device..."
    scons -j $cpus $options $platform target=template_debug arch=arm64 ios_simulator=no
    mv $BINDIR/ios.framework/libspine_godot.ios.template_debug $BINDIR/libspine_godot.ios.template_debug.a

    scons -j $cpus $options $platform target=template_release arch=arm64 ios_simulator=no
    mv $BINDIR/ios.framework/libspine_godot.ios.template_release $BINDIR/libspine_godot.ios.template_release.a

    # Step 3: Create xcframeworks
    echo "Creating xcframeworks..."

    xcodebuild -create-xcframework \
        -library $BINDIR/libspine_godot.ios.template_debug.a \
        -library $BINDIR/libspine_godot.ios.template_debug.simulator.a \
        -output $BINDIR/libspine_godot.ios.template_debug.xcframework

    xcodebuild -create-xcframework \
        -library $BINDIR/libspine_godot.ios.template_release.a \
        -library $BINDIR/libspine_godot.ios.template_release.simulator.a \
        -output $BINDIR/libspine_godot.ios.template_release.xcframework

    # Cleanup intermediate files
    rm -f $BINDIR/*.a
    rm -rf $BINDIR/ios.framework

elif [ "$raw_platform" == "macos" ]; then
    BINDIR="example-v4-extension/bin/macos/macos.framework"
    TMPDIR="example-v4-extension/bin/macos/tmp"
    mkdir -p $BINDIR $TMPDIR

    # Build x86_64 binaries
    echo "Building for macOS x86_64..."
    scons -j $cpus $options $platform target=editor arch=x86_64
    mv $BINDIR/libspine_godot.macos.editor $TMPDIR/libspine_godot.macos.editor.x86_64
    scons -j $cpus $options $platform target=template_debug arch=x86_64
    mv $BINDIR/libspine_godot.macos.template_debug $TMPDIR/libspine_godot.macos.template_debug.x86_64
    scons -j $cpus $options $platform target=template_release arch=x86_64
    mv $BINDIR/libspine_godot.macos.template_release $TMPDIR/libspine_godot.macos.template_release.x86_64

    # Build arm64 binaries
    echo "Building for macOS arm64..."
    scons -j $cpus $options $platform target=editor arch=arm64
    mv $BINDIR/libspine_godot.macos.editor $TMPDIR/libspine_godot.macos.editor.arm64
    scons -j $cpus $options $platform target=template_debug arch=arm64
    mv $BINDIR/libspine_godot.macos.template_debug $TMPDIR/libspine_godot.macos.template_debug.arm64
    scons -j $cpus $options $platform target=template_release arch=arm64
    mv $BINDIR/libspine_godot.macos.template_release $TMPDIR/libspine_godot.macos.template_release.arm64

    # Create universal binaries
    echo "Creating universal binaries..."
    lipo -create \
        $TMPDIR/libspine_godot.macos.editor.x86_64 \
        $TMPDIR/libspine_godot.macos.editor.arm64 \
        -output $BINDIR/libspine_godot.macos.editor

    lipo -create \
        $TMPDIR/libspine_godot.macos.template_debug.x86_64 \
        $TMPDIR/libspine_godot.macos.template_debug.arm64 \
        -output $BINDIR/libspine_godot.macos.template_debug

    lipo -create \
        $TMPDIR/libspine_godot.macos.template_release.x86_64 \
        $TMPDIR/libspine_godot.macos.template_release.arm64 \
        -output $BINDIR/libspine_godot.macos.template_release

    # Cleanup intermediate files
    rm -rf $TMPDIR

elif [ "$raw_platform" == "web" ]; then
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
    if [ "$raw_platform" != "android" ] && [ "$raw_platform" != "web" ]; then
        scons -j $cpus $options $platform target=editor
    fi
    scons -j $cpus $options $platform target=template_debug
    scons -j $cpus $options $platform target=template_release
fi

popd

popd