# spine-cocos-creator v3.x

The spine-cocos-creator runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos-creator](https://www.cocos.com/). spine-cocos-creator is based on [cocos engine](https://github.com/cocos/cocos-engine).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-cocos-creator works with data exported from Spine 3.8.xx.

spine-cocos-creator supports all Spine features.

### Cocos Creator v3.8.x

You don't need to make any modifications. Currently, the engine supports spine internally.

Here are the steps if you want to customize the function.

1. Download the cocos creator engine.
2. Modify Spine-related codes in engine and editor-support.
3. Compile wasm & asm files.
   1. Spine wasm compilation. To build spine wasm, first install the emscripten sdk.
   2. The engine/native/cocos/editor-support/spine-wasm directory contains the source code and compilation configuration for building the spine wasm project.
   3. Create a new build directory under the spine-wasm directory, and execute emcmake cmake .. in it to generate a Build file.
   4. Execute emmake make in the build directory.
   5. Configure the CMakeLists.txt file

       **WASM=0** `emmake make` compiles the **asm** version, and the compiled products are spine.js and spine.js.mem. Modify the file name spine.js to spine.asm.js name. Copy `spine.js.mem` and `spine.js.mem` to the engine/native/external/emscripten/spine directory.

       **WASM=1** `emmake make` compiles the **wasm** version, and the compiled products are spine.js and spine.wasm. Modify the file name spine.js to spine.wasm.js. Copy `spine.wasm.js` and `spine.wasm` to the engine/native/external/emscripten/spine directory.

4. Compile and load the engine code.