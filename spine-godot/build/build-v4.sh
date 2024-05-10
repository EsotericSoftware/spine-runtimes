#!/bin/bash
set -e

dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd "$dir" > /dev/null

if [ ! -d ../godot ]; then
	echo "No Godot clone found. Run ./setup.sh <Godot branch or tag> <dev> first."
	exit 1
fi

target=""
dev="false"
mono="false"

if [ $# -gt 0 ]; then
	if [ $# -gt 1 ]; then
		echo "Usage: $0 <mono:true|false>"
		exit 1
	else
		if [ "$1" == "true" ] || [ "$1" == "false" ]; then
			mono="$1"
		else
			echo "Invalid value for the 'mono' argument. It should be either 'true' or 'false'."
			exit 1
		fi
	fi
fi

if [ -f "../godot/custom.py" ]; then
	dev="true"
	echo "DEV build"
fi

mono_module=""
mono_extension=""
if [ $mono == "true" ]; then
	mono_module="module_mono_enabled=yes"
	mono_extension=".mono"
	echo "Building Godot with C# support"
else
	echo "Building Godot without C# support"
fi

dev_extension=""
if [ $dev == "true" ]; then
	dev_extension=".dev"
	target="$target dev_build=true"
fi

cpus=2
if [ "$OSTYPE" == "msys" ]; then
	os="windows"
	cpus=$NUMBER_OF_PROCESSORS
	target="$target"
	godot_exe="godot.windows.editor$dev_extension.x86_64$mono_extension.exe"
	godot_exe_host=$godot_exe
elif [[ "$OSTYPE" == "darwin"* ]]; then
	os="macos"
	cpus=$(sysctl -n hw.logicalcpu)
	godot_exe="godot.macos.editor$dev_extension.x86_64$mono_extension"
	godot_exe_arm="godot.macos.editor$dev_extension.arm64$mono_extension"
	godot_exe_host=$godot_exe
	if [ `uname -m` == "arm64" ]; then
		godot_exe_host=$godot_exe_arm
	fi
else
	os="linux"
	cpus=$(grep -c ^processor /proc/cpuinfo)
	godot_exe="godot.linuxbsd.editor$dev_extension.x86_64$mono_extension"
	godot_exe_host=$godot_exe
fi

echo "CPUS: $cpus"

pushd ../godot
if [ "$os" == "macos" ] && [ $dev == "false" ]; then
	scons $target $mono_module arch=x86_64 compiledb=yes custom_modules="../spine_godot" opengl3=yes --jobs=$cpus
	scons $target $mono_module arch=arm64 compiledb=yes custom_modules="../spine_godot" opengl3=yes --jobs=$cpus
	if [ $mono == "true" ]; then
		echo "Building C# glue and assemblies."
		"./bin/$godot_exe_host" --generate-mono-glue modules/mono/glue
		python3 ./modules/mono/build_scripts/build_assemblies.py --godot-output-dir ./bin --push-nupkgs-local ../godot-spine-csharp
	fi
	pushd bin
	cp -r ../misc/dist/macos_tools.app .
	mv macos_tools.app Godot.app
	mkdir -p Godot.app/Contents/MacOS
	lipo -create $godot_exe_arm $godot_exe -output Godot
	strip -S -x Godot
	cp Godot Godot.app/Contents/MacOS/Godot
	chmod +x Godot.app/Contents/MacOS/Godot
	if [ $mono == "true" ]; then
		cp -r GodotSharp Godot.app/Contents/Resources
	fi
	popd
else
	scons $target $mono_module compiledb=yes custom_modules="../spine_godot" opengl3=yes --jobs=$cpus
	if [ $mono == "true" ]; then
		echo "Building C# glue and assemblies."
		"./bin/$godot_exe_host" --headless --generate-mono-glue modules/mono/glue
		python3 ./modules/mono/build_scripts/build_assemblies.py --godot-output-dir ./bin --push-nupkgs-local ../godot-nuget
	fi
	cp compile_commands.json ../build
	if [ -f "bin/godot.linuxbsd.editor.x86_64$mono_extension" ]; then
		strip bin/godot.linuxbsd.editor.x86_64$mono_extension
		chmod a+x bin/godot.linuxbsd.editor.x86_64$mono_extension
	fi
fi
popd

popd > /dev/null