# spine-xna

The spine-xna runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [XNA](http://msdn.microsoft.com/xna/). spine-xna is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-xna works with data exported from Spine 3.6.xx.

spine-xna supports all Spine features.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip).
1. For XNA with Visual Studio 2015, download [XNA 4.0 refresh for Visual Studio 2015](https://mxa.codeplex.com/releases/view/618279). Install each subfolder as per the README file.   
1. Open the `spine-xna.sln` project file with Visual Studio.
1. Set `spine-xna-example` as the startup project
1. Set the working directory of `spine-xna-example` to `spine-runtimes/spine-xna/example`
1. Run the example!

Alternatively, the contents of the `spine-csharp/src` and `spine-xna/src` directories can be copied into your project.
