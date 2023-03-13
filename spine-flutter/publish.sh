#!/bin/sh
set -e
./setup.sh
./compile-wasm.sh
dart pub publish --dry-run
dart pub publish