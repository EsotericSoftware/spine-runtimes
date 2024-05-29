# spine-sdl

The spine-sdl runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [SDL](https://www.libsdl.org/). spine-sdl is based on [spine-c](../spine-c) and [spine-cpp](../spine-cpp), depending on whether you want to use a C or C++ implementation.

# See the [spine-sdl documentation](http://esotericsoftware.com/spine-sdl) for in-depth information

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-sdl works with data exported from Spine 4.2.xx.

spine-sdl supports all Spine features except screen blend mode and two color tinting.

## Usage

1. Create a new SDL project. See the [SDL documentation](https://wiki.libsdl.org/FrontPage) or have a look at the example in this repository.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
3. If you are using C, add the sources from `spine-c/spine-c/src/spine` and `spine-sdl/src/spine-sdl-c.c` to your project, and add the folder `spine-c/spine-c/include` and `spine-sdl/src/spine-sdl-c.h` to your header search path. Note that includes are specified as `#inclue <spine/file.h>`, so the `spine` directory cannot be omitted when copying the source files.
4. If you are using C++, add the sources from `spine-cpp/spine-cpp/src/spine` and `spine-sdl/src/spine-sdl-cpp.cpp` to your project, and add the folder `spine-cpp/spine-cpp/include` and `spine-sdl/src/spine-sdl-cpp.h` to your header search path. Note that includes are specified as `#include <spine/file.h>`, so the `spine` directory cannot be omitted when copying the source files.

See the [Spine Runtimes documentation](http://esotericsoftware.com/spine-documentation#runtimesTitle) on how to use the APIs or check out the Spine SDL example in this repository.

## Example

The Spine SDL example works on Windows, Linux and Mac OS X. For a spine-c based example, see [example/main.c](example/main.c), for a spine-cpp example see [example/main.cpp](example/main.cpp).

### Windows

1. Install [Visual Studio Community](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx). Make sure you install support for C++ as well as th Windows SDK for XP/7/8.
2. Install CMake via the [Windows installer package](https://cmake.org/download/).
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Run CMake GUI from the start menu
5. Click `Browse Source` and select the directory `spine-runtimes`
6. Click `Browse Build` and select the `spine-runtimes/spine-sdl/build` directory. You can create the `build` folder directly in the file dialog via `New Folder`.
7. Click `Configure`. Then click `Generate`. This will create a Visual Studio solution file called `spine.sln` in `spine-runtimes/spine-sdl/build` and also download the SDL dependencies.
8. Open the `spine.sln` file in Visual Studio
9. Right click the `spine-sdl-example-c` or `spine-sdl-example-cpp` project in the solution explorer and select `Set as Startup Project` from the context menus
10. Click `Local Windows Debugger` to run the example

The entire example code is contained in [main.cpp](example/main.cpp#L61)

### Linux

1. Install the [SDL build dependencies](https://github.com/libsdl-org/SDL/blob/main/docs/README-linux.md)
2. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
3. Open a terminal, and `cd` into the `spine-runtimes/spine-sdl` folder
4. Type `mkdir build && cd build && cmake ../..` to generate Make files
5. Type `make` to compile the example
6. Run the example by `cd spine-sdl && ./spine-sdl-c-example` (C) or by `cd spine-sdl && ./spine-sdl-cpp-example` (C++)

### Mac OS X

1. Install [Xcode](https://developer.apple.com/xcode/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
4. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
5. Open a terminal, and `cd` into the `spine-runtimes/spine-sdl` folder
6. Type `mkdir build && cd build && cmake ../..` to generate Make files
7. Type `make` to compile the example
8. Run the example by `cd spine-sdl && ./spine-sdl-c-example` (C) or by `cd spine-sdl && ./spine-sdl-cpp-example` (C++)
