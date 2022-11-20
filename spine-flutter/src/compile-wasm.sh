#!/bin/sh
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null
mkdir -p ../lib/assets/
emcc \
	-Ispine-cpp/include \
	--closure 1 -O3 -fno-rtti -fno-exceptions \
	-s MAIN_MODULE=1 \
	-s MODULARIZE=1 \
	-s ALLOW_MEMORY_GROWTH=1 \
	-s ALLOW_TABLE_GROWTH \
	-s MALLOC=emmalloc \
	-s ENVIRONMENT=web \
	--no-entry \
	-s EXPORT_NAME=libspine_flutter \
	spine_flutter.cpp `find spine-cpp/src -type f` \
	-o ../lib/assets/libspine_flutter.js
	ls -lah ../lib/assets
popd