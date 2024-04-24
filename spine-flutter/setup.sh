#!/bin/bash
set -e
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null
# Need to copy spine-cpp sources to the ios and macos folders, as CocoaPods requires
# all source files to be under the same folder hierarchy the podspec file resides in.
cp -r ../spine-cpp/spine-cpp ios/Classes
cp -r ../spine-cpp/spine-cpp macos/Classes
cp -r ../spine-cpp/spine-cpp src
cp -r ../spine-cpp/spine-cpp-lite src
popd