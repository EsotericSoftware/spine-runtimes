# spine-godot

The spine-godot runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Godot](https://godotengine.org/). spine-godot is based on [spine-cpp](../spine-cpp).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-godot works with data exported from Spine 4.2.xx.

spine-godot supports all Spine features, except two-color tinting and the screen blend mode.

## Setup

spine-godot works with the latest stable Godot 3.4 release. It requires compilation of Godot, as spine-godot is implemented as a module.

To integrate spine-godot into your project:

1. Follow the [instructions on how to compilation of Godot](https://docs.godotengine.org/en/stable/development/compiling/index.html)
2. Copy the `spine-runtimes/spine-godot/spine_godot` folder into the folder `modules/spine_godot` in your Godot source tree.
3. Copy the `spine-cpp/spine-cpp`folder into the folder `modules/spine_godot/spine-cpp` in your Godot source tree.
4. Compile Godot via scons for your platform as per the Godot documentation.

The resulting Godot engine binary will include the spine-godot runtime.

## Example
Install the [software required to build Godot] for your operating system. This generally means:

1. Git
1. A C++ compiler (MSVC, Clang, GCC)
1. Python 3+
1. Scons

Then execute the `setup.bat` file on Windows, or the `setup.sh` file on Linux and macOS. After the script completes, you should find the Godot editor binary in `spine-runtimes/spine-godot/godot/bin`. Run it, and open the Godot example project in `spine-runtimes/spine-godot/example`.





