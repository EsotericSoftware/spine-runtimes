# spine-sfml

The spine-sfml runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [SFML](http://www.sfml-dev.org/). spine-sfml is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-sfml works with data exported from Spine 3.6.xx.

spine-sfml supports all Spine features.

spine-sfml does not yet support loading the binary format.

## Usage
1. Create a new SFML project. See the [SFML documentation](http://www.sfml-dev.org/tutorials/2.1/) or have a look at the example in this repository.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
3. Add the sources from `spine-c/spine-c/src/spine` and `spine-sfml/src/spine` to your project
4. Add the folder `spine-c/spine-c/include` to your header search path. Note that includes are specified as `#inclue <spine/file.h>`, so the `spine` directory cannot be omitted when copying the source files.

See the [Spine Runtimes documentation](http://esotericsoftware.com/spine-documentation#runtimesTitle) on how to use the APIs or check out the Spine SFML example.

## Example
The Spine SFML example works on Windows, Linux and Mac OS X.

### Windows
1. Install [Visual Studio 2015 Community](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx). Make sure you install support for C++ as well as th Windows SDK for XP/7/8.
2. Install CMake via the [Windows installer package](https://cmake.org/download/).
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Run CMake GUI from the start menu
5. Click `Browse Source` and select the directory `spine-runtimes`
6. Click `Browse Build` and select the `spine-runtimes/spine-sfml/build` directory. You can create the `build` folder directly in the file dialog via `New Folder`.
7. Click `Configure`. Then click `Generate`. This will create a Visual Studio 2015 solution file called `spine.sln` in `spine-runtimes/spine-sfml/build` and also download the SFML dependencies.
8. Open the `spine.sln` file in Visual Studio 2015
9. Right click the `spine-sfml-example` project in the solution explorer and select `Set as Startup Project` from the context menu
10. Right click the `spine-sfml-example` project in the solution explorer and select `Properties` from the context menu
11. Select `Debugging` in the left-hand list, then set `Working Directory` to `$(OutputPath)`
12. Click `Local Windows Debugger` to run the example

The entire example code is contained in [main.cpp](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-sfml/example/main.cpp#L61)

### Linux
1. Install the SFML dependencies, e.g. on Ubuntu/Debian via `sudo apt-get install -y libpthread-stubs0-dev libgl1-mesa-dev libx11-dev libxrandr-dev libfreetype6-dev libglew1.5-dev libjpeg8-dev libsndfile1-dev libopenal-dev libudev-dev libxcb-image0-dev libjpeg-dev libflac-dev`
2. Install CMake, e.g. on Ubuntu/Debian via `sudo apt-get install -y cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Open a terminal, and `cd` into the `spine-runtimes/spine-sfml` folder
5. Type `mkdir build && cd build && cmake ../..` to generate Make files
6. Type `make` to compile the example
7. Run the example by `cd spine-sfml-example && ./spine-sfml-example`

### Mac OS X
1. Install [Xcode](https://developer.apple.com/xcode/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Open a terminal, and `cd` into the `spine-runtimes/spine-sfml` folder
5. Type `mkdir build && cd build && cmake -G Xcode ../..` to generate an Xcode project called `spine.xcodeproj`
6. Open the Xcode project in `spine-runtimes/spine-sfml/build/`
7. In Xcode, set the active scheme from `ALL_BUILD` to `spine-sfml-example`
8. Click the `Run` button or type `CMD+R` to run the example

## Notes

- Atlas images should not use premultiplied alpha.
