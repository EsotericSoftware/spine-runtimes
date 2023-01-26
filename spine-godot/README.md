# spine-godot

The spine-godot runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Godot](https://godotengine.org/). spine-godot is based on [spine-cpp](../spine-cpp).

# See the [spine-godot documentation](http://esotericsoftware.com/spine-godot) for in-depth information.

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-godot works with data exported from Spine 4.1.xx.

spine-godot supports all Spine features, except two-color tinting and the screen blend mode.

## Setup

spine-godot works with the latest stable Godot 3.5 release. It requires compilation of Godot, as spine-godot is implemented as a module.

> *NOTE:* spine-godot also compiles and works against Godot 4.x. However, we currently can not guarantee stability, as Godot 4.x is still heavily in flux.

### Pre-built Godot editor and export template binaries

We provide prebuilt Godot editor and export template binaries for Godot 3.5.1-stable:

* [Godot Editor Windows](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/3.5.1-stable/godot-editor-windows.zip)
* [Godot Editor Linux](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/3.5.1-stable/godot-editor-linux.zip)
* [Godot Editor macOS](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/3.5.1-stable/godot-editor-macos.zip)
* [Godot export templates for Windows, Linux, macOS, Web, Android, iOS](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/3.5.1-stable/spine-godot-templates-4.1-3.5-stable.tpz)

We also provide prebuilt Godot editor and export template binaries for the Godot master branch:

* [Godot 4.0 Editor Windows](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/master/godot-editor-windows.zip)
* [Godot 4.0 Editor Linux](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/master/godot-editor-linux.zip)
* [Godot 4.0 Editor macOS](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/master/godot-editor-macos.zip)
* [Godot 4.0 Export templates for Windows, Linux, macOS, Web, Android, iOS](https://spine-godot.s3.eu-central-1.amazonaws.com/4.1/master/spine-godot-templates-4.1-master.tpz)

Note that Godot 4.0 is unstable. Use Godot 3.5.1 for production for now.

### Building the Godot editor and export templates locally

If you want to build the Godot editor and export templates yourself, either because you want to add custom engine modules or for engine development, you can use the shell scripts in the `spine-godot/build` folder. 

> *NOTE:* Make sure you have all build dependencies installed before attempting this, as per the [official instructions by Godot](https://docs.godotengine.org/en/stable/development/compiling/index.html).

To build a Godot editor binary, run the following in a Bash shell (use Git Bash on Windows):

```
cd spine-godot
./build/setup.sh 3.4.4-stable false
./build/build.sh release_debug
```

The first argument to `setup.sh` is the Godot repository branch or commit hash you want to build the Godot editor from. The second argument specifies whether you want to compile the editor for development, which will include support for Live++ on Windows, and disable many modules so compilation is faster.

The first argument to `build.sh` specifies the optimization level of the resulting binary. Supported values are `debug`, which allows full debugging but may be slow due to missing optimizations, and `release_debug`, which is generally used for release builds of the Godot editor.

The resulting Godot editor binary can then be found in `spine-godot/godot/bin`.

To build the export template for a specific platform, run the following in a Bash shell (use Git Bash on Windows):

```
cd spine-godot
./build/setup.sh 3.5-stable false
./build/build-templates.sh windows
```

The first argument to `setup.sh` is the Godot repository branch or commit hash you want to build the Godot editor from. The second argument must be `false`.

The first argument to `built-templates.sh` is the platform to compile the template for. Valid values are `windows`, `linux`, `macos`, `web`, `android`, and `ios`. Note that macOS and iOS export templates can only be build on a machine running macOS. See the [official instructions by Godot](https://docs.godotengine.org/en/stable/development/compiling/index.html).

The resulting Godot export template binary can then be found in `spine-godot/godot/bin`.

### Building the Godot editor and export templates via GitHub Actions
This repository contains a GitHub workflow in `.github/workflows/spine-godot.yml` and `.github/workflows/spine-godot-v4.yml` that allows building all Godot editor and export template binaries through GitHub actions for Godot 3.5.x and Godot 4.0. This may be simpler than compiling these artifacts locally. To use the GitHub workflow:

1. Clone this repository
2. Enable GitHub workflows on the cloned repository
3. Manually trigger the `spine-godot` or `spine-godot-v4` workflow.

The resulting binaries will be attached as artifacts to a successful workflow run.

## Example
Sample projects for both Godot 3.5.x and Godot 4.x are provided in the `example/` and `example-v4/` folders respectively. They illustrate all spine-godot functionality and can be opened and exported with the pre-built or custom build Godot editor and export template binaries.



