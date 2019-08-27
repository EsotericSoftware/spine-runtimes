# spine-c-unit-tests

The spine-c-unit-tests project is to test the [Spine](http://esotericsoftware.com) skeletal animation system. It does not perform rendering.  It is primarily used for regression testing and leak detection.  It is designed to be run from a Continuous Integration server and to passively verify changes automatically on check-in.

## Mini CPP Unit Testing
[MiniCppUnit](https://sourceforge.net/p/minicppunit/wiki/Home/) is a minimal unit testing framework similar to JUnit.  It is used here to avoid large dependancies.

Tests are sorted into Suites, Fixtures and Cases.  There is one suite, it contains many fixtures and each fixture contains test cases.  To turn off a fixture, edit "TestOptions.h".  To turn off specific test cases, comment out the TEST_CASE() line in the fixture's header.

## Memory Leak Detection
This project includes a very minimal memory leak detector.  It is based roughly on the leak detector in the [Popcap Framework](https://sourceforge.net/projects/popcapframework/?source=directory), but has been modified over the years.

## Continuous Integration
The test runner includes the ability to format output messages to signal a CI server.  An example interface for [Teamcity](https://www.jetbrains.com/teamcity/) is included.  To implement for another server, determine the wireformat for the messages and duplicate/edit the teamcity_messages class. [Teamcity Wire Format](https://confluence.jetbrains.com/display/TCD10/Build+Script+Interaction+with+TeamCity)

### Trigger
Your CI server should trigger on VCS check-in.

### CMake Build Step
The first build step for the CI server should be to run CMake on the 'spine-c-unit-tests' folder.  Follow the usage directions below.

### Compile Build Step
This build step should not execute if the previous step did not successfully complete.
Depending on the test agent build environment, you should build the output solution or project from the cmake step.  Debug is fine.

### Test Runner Build Step
This build step should not execute if the previous step did not successfully complete.
Again, depending on the test agent build environment, you should have produced an executable.  Run this executable.


## Usage
Make sure [CMake](https://cmake.org/download/) is installed.

Create a 'build' directory in the 'spine-c-unit-tests' folder.  Then switch to that folder and execute cmake:

mkdir build
cd build
cmake ..

### Win32 build
msbuild spine_unit_test.sln /t:spine_unit_test /p:Configuration="Debug" /p:Platform="Win32"


## Licensing
This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

original "walk"": 330
second "walk": 0d0

queue interrupt for original walk
queue start for second walk
drain interrupt and start

0d0 is interrupted
0d0 is ended

"run": 0c0
 0d0 is interrupted
 second walk becomes mixingFrom of run
 0c0 is started

 queue is drained

 first walk: 6f0
 second walk: 9c0
