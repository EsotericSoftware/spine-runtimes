# spine-xna

The spine-xna runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [XNA](http://msdn.microsoft.com/xna/). spine-xna is based on [spine-csharp](../spine-csharp).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-xna works with data exported from Spine 3.8.xx.

spine-xna supports all Spine features.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
1. For XNA with Visual Studio 2015, download [XNA 4.0 refresh for Visual Studio 2015](https://mxa.codeplex.com/releases/view/618279). Install each subfolder as per the README file.
1. Open the `spine-xna.sln` project file with Visual Studio.
1. Set `spine-xna-example` as the startup project
1. Set the working directory of `spine-xna-example` to `spine-runtimes/spine-xna/example`
1. Run the example!

Alternatively, the contents of the `spine-csharp/src` and `spine-xna/src` directories can be copied into your project.
