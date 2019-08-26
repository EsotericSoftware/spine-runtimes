# spine-cocos-creator

The spine-cocos-creator runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data in [Cocos Creator](http://www.cocos2d-x.org/). spine-cocos-creator is based on [spine-cpp](../spine-cpp).

Cocos Creator is a unified game creation tool with a full featured editor based on entity component architecture, it supports JavaScript and TypeScript as development languages.

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](http://esotericsoftware.com/git/spine-runtimes/blob/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-cocos-creator works with data exported from Spine 3.8.xx.

spine-cocos-creator supports all Spine features.

## Documentation

A Spine skeleton node (a node with a Skeleton component on it) can be used throughout Cocos Creator like any other node.

## Testing (Temporary)

This section is for getting things working in the current state and will be updated once this pull request get merged.

As the official version of editor with the correct code base haven't been published yet, we used an internal version of Cocos Creator to build an example case running the code provided by this pull request. You can follow the instructions to make it running:

1. Download [the example project](https://digitalocean.cocos2d-x.org/spine/spine-example.7z) and unarchive the file
2. Find the projects for Xcode, Visual Studio and Android Studio in `frameworks/runtime-src/`.
3. Build for your desired platform and test.

The Spine runtime code corresponding this pull request is locating at `frameworks/cocos2d-x/cocos/editor-support/spine-creator-support`.

## The official workflow

After this pull request get merged, and the next version of Cocos Creator (v2.2.0) get released, user can finally benefit the C++ Spine runtime and new features in v3.8.

A ordinary workflow for using Spine in Cocos Creator is described below:

1. Drag the Spine asset (json atlas png) into the asset panel of Cocos Creator.
2. Add a Spine Skeleton component to a node, and attach the Spine animation asset onto it.
3. Config the animation properties in the inspector panel.
4. Run the game to see how it works.

See the [Spine Skeleton component reference](https://docs.cocos2d-x.org/creator/manual/en/components/spine.html).