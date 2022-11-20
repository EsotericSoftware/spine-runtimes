#!/bin/sh
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null
emcc -Ispine-cpp/include --closure 1 -O3 -fno-rtti -fno-exceptions -s MAIN_MODULE=1 -s EXPORT_NAME=libspine_flutter -s MODULARIZE=1 spine_flutter.cpp `find spine-cpp/src -type f` -o ../lib/assets/libspine_flutter.js
popd