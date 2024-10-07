# spine-haxe

The spine-haxe runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Haxe](https://haxe.org/) in combination with [OpenFL](https://www.openfl.org/) and [Lime](https://lime.openfl.org/).

For documentation of the core API in `spine-core`, please refer to our [Spine Runtimes Guide](http://esotericsoftware.com/spine-runtimes-guide).

For documentation of `spine-haxe`, please refer to our [spine-haxe Guide](https://esotericsoftware.com/spine-haxe).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-haxe works with data exported from Spine 4.2.xx.

spine-haxe supports all Spine features except premultiplied alpha atlases and two color tinting.

## Setup
The core module of spine-haxe has zero dependencies. The rendering implementation through Starling has two dependencies: openfl and starling.
To use spine-haxe you have first to install all the necessary dependencies:

```
haxelib install openfl
haxelib install starling
```

Once you have installed the dependencies, you can [download the latest version of spine-haxe](https://esotericsoftware.com/files/spine-haxe/4.2/spine-haxe-latest.zip) and install it:

```
haxelib install spine-haxe-x.y.z.zip
```

Notice that the spine-haxe library is not available on [lib.haxe.org](https://lib.haxe.org/). This is why you need to download the library and install it through the zip archive.

## Example

The `example/` folder contains the spine-haxe examples. They demonstrates the usage of the API. Enter the the `spine-haxe` folder and run the following command:

```
lime test html5
```

This will compile the modules and start a server that serves the example pages at http://127.0.0.1:3000.

## Development

To setup the development environment install the following:

1. [Haxe](https://haxe.org/download/), ensure it's available on the command line through your `PATH` if you use the binaries instead of the installer.
2. On the command line, run:
   ```
   haxelib setup
   haxelib install openfl
   haxelib run openfl setup
   haxelib install starling
   ```
3. Clone the `spine-runtimes` repository, and use `haxelib` to setup a dev library:
   ```
   git clone https://github.com/esotericsoftware/spine-runtimes
   cd spine-runtimes
   haxelib dev spine-haxe .
   ```

As an IDE, we recommend [Visual Studio Code](https://code.visualstudio.com/) with the following extension:

1. [Haxe extension](https://marketplace.visualstudio.com/items?itemName=nadako.vshaxe)
2. [HXCPP debugger extension](https://marketplace.visualstudio.com/items?itemName=vshaxe.hxcpp-debugger)
3. [Lime extension](https://marketplace.visualstudio.com/items?itemName=openfl.lime-vscode-extension)

The extensions provide IDE features like auto-completion, debugging, and build support.

To debug a build, set the corresponding Lime target in the status bar at the bottom of VS Code to e.g. `HTML5 / Debug`. Run the `lime` run configuration by pressing `F5`.
