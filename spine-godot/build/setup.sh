#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir" > /dev/null

if [ $# -lt 2 ] || [ $# -gt 4 ]; then
	echo "Usage: ./setup.sh <Godot branch or tag> <dev:true|false> <mono:true|false>? <godot-repository>?"
	echo
	echo "e.g.:"
	echo "       ./setup.sh 4.2.1-stable true"
	echo "       ./setup.sh master false true"
	echo "       ./setup.sh master false false https://github.com/my-github-username/godot.git"
	echo
	echo "Note: the 'mono' parameter only works for Godot 4.x+!"

	exit 1
fi

branch=${1%/}
dev=${2%/}
mono=false
repo=https://github.com/godotengine/godot.git
version=$(echo $branch | cut -d. -f1-2)
echo $version > version.txt

if [[ $# -eq 3 && "$branch" != 3* ]]; then
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

if [ $# -eq 4 ]; then
    repo=${4%/}
fi

pushd ..
rm -rf godot
git clone --depth 1 $repo -b $branch

if [ $dev = "true" ]; then
	cp build/custom.py godot
	if [ "$mono" = "true" ]; then
		echo "" >> godot/custom.py
    	echo "module_mono_enabled=\"yes\"" >> godot/custom.py
	fi
	cp ../formatters/.clang-format .
	rm -rf example/.import
	rm -rf example/.godot

	if [ `uname` == 'Darwin' ] && [ ! -d "$HOME/VulkanSDK" ]; then
		./build/install-macos-vulkan-sdk.sh
	fi
fi
rm -rf spine_godot/spine-cpp
cp -r ../spine-cpp spine_godot

# Apply patch for 4.3-stable, see https://github.com/godotengine/godot/issues/95861/#issuecomment-2486021565
if [ "$branch" = "4.3-stable" ]; then
    pushd godot
    cp ../build/4.3-stable/tvgLock.h thirdparty/thorvg/src/common/tvgLock.h
	cp ../build/4.3-stable/tvgTaskScheduler.h thirdparty/thorvg/src/renderer/tvgTaskScheduler.h
    popd
fi

# Apply iOS Vulkan fix for C++ module imports in extern "C" blocks
# This fixes compilation errors with newer Xcode versions on Godot 4.x
if [[ "$branch" == 4* ]]; then
    pushd godot
    if [ -f platform/ios/detect.py ]; then
        # Check if the fix is already applied
        if ! grep -q "Wno-error=module-import-in-extern-c" platform/ios/detect.py; then
            # Insert the fix after the -Wno-ambiguous-macro line
            sed -i.bak '/Wno-ambiguous-macro/a\
\
    # Fix for C++ module imports in extern "C" blocks in Vulkan headers\
    env.Append(CCFLAGS=["-Wno-error=module-import-in-extern-c"])
' platform/ios/detect.py && rm platform/ios/detect.py.bak
            echo "Applied iOS Vulkan fix to platform/ios/detect.py"
        fi
    fi
    popd
fi

popd

# Generate compile_commands.json for IDE integration (only in dev mode)
if [ $dev = "true" ]; then
	echo "Generating compile_commands.json for IDE integration..."
	pushd ../godot > /dev/null

	# Detect architecture for macOS
	arch=""
	platform=""
	if [[ "$OSTYPE" == "darwin"* ]]; then
		if [ `uname -m` == "arm64" ]; then
			arch="arch=arm64"
		else
			arch="arch=x86_64"
		fi
		platform="platform=osx"
	elif [[ "$OSTYPE" == "msys"* ]] || [[ "$OSTYPE" == "win32"* ]]; then
		platform="platform=windows"
	else
		# Linux - only generate if X11 dev libraries are available
		if pkg-config --exists x11 2>/dev/null; then
			platform="platform=x11"
		else
			echo "Skipping compile_commands.json generation (X11 libraries not available)"
			popd > /dev/null
			popd > /dev/null
			exit 0
		fi
	fi

	# Generate compilation database without building
	scons compiledb=yes custom_modules="../spine_godot" opengl3=yes $platform $arch compiledb

	# Copy to parent directory for easy IDE access
	if [ -f compile_commands.json ]; then
		cp compile_commands.json ..
		echo "compile_commands.json generated successfully and copied to spine-godot/"
	else
		echo "Warning: Failed to generate compile_commands.json"
	fi

	popd > /dev/null
fi

popd > /dev/null