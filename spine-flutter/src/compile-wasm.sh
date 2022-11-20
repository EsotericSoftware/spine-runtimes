#!/bin/sh
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
pushd $dir > /dev/null
mkdir -p ../lib/assets/
# Need to use -O2, as -O3 applies the Closure compiler to native function names.
# The entries for exported functions in Module.asm will be scrambled so
# EmscriptenModule._fromJs() is unable to parse them and link them with original
# names set on the module, e.g. Module._spine_get_major_version.
emcc \
	-Ispine-cpp/include \
	-O2 --closure 1 -fno-rtti -fno-exceptions -lc++abi -lc++ \
	-s STRICT=1 \
	-s LLD_REPORT_UNDEFINED \
	-s MODULARIZE=1 \
	-s ALLOW_MEMORY_GROWTH=1 \
	-s ALLOW_TABLE_GROWTH \
	-s MALLOC=emmalloc \
	-s EXPORT_ALL=1 \
	-s EXPORTED_FUNCTIONS='["_malloc", "_free"]' \
	--no-entry \
	-s EXPORT_NAME=libspine_flutter \
	spine_flutter.cpp `find spine-cpp/src -type f` \
	-o ../lib/assets/libspine_flutter.js
	ls -lah ../lib/assets
popd