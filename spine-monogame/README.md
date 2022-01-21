# spine-monogame

The spine-monogame runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [MonoGame](http://monogame.codeplex.com/). spine-monogame is based on [spine-xna](../spine-xna) and adds MonoGame project files.

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-monogame works with data exported from Spine 4.1.xx.

spine-monogame supports all Spine features.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
1. Let your MonoGame project reference the project `spine-monogame/spine-monogame/spine-monogame.csproj`.
1. Optionally add `spine-monogame/spine-monogame-example/Content/shaders/SpineEffect.fx` to your content pipeline build if you require two color tinting support. See the `spine-monogame-example/ExampleGame.cs` on how to use it.

Note: `spine-monogame` references source files from `spine-csharp/src`.

## Example

1. Follow the [MonoGame Getting Started Guide](https://docs.monogame.net/articles/getting_started/0_getting_started.html) on how to setup your development environment for MonoGame. Make sure to also install the optional dependencies allowing compilation of shaders. 
1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip via the download button above.
1. Open the `spine-monogame.sln` Solution in the IDE you choose based on the `Monogame Getting Started Guide`.
1. Set the `spine-monogame-example` project as the startup project, and ensure the working directory is set to `spine-runtimes/spine-monogame/spine-monogame-example` when running or debugging the project.
