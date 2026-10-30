#!/bin/bash
set -e
cd "$(dirname "$0")"

# Source logging utilities
source ../formatters/logging/logging.sh

log_title "Spine Flutter Setup"

log_detail "CocoaPods requires all source files to be under the same folder hierarchy"
log_detail "as the podspec file resides in. Copying spine-cpp sources to platform folders."

# Clean destination directories
log_action "Cleaning destination directories"
rm -rf ios/spine_flutter/Sources/spine_flutter/spine-c ios/spine_flutter/Sources/spine_flutter/spine-cpp
rm -rf macos/spine_flutter/Sources/spine_flutter/spine-c macos/spine_flutter/Sources/spine_flutter/spine-cpp
rm -rf src/spine-c src/spine-cpp
# Legacy (pre Swift Package Manager) locations
rm -rf ios/Classes macos/Classes
log_ok

log_action "Copying spine-c to iOS package"
mkdir -p ios/spine_flutter/Sources/spine_flutter/spine-c
if CP_OUTPUT=$(cp -r ../spine-c/src ../spine-c/include ios/spine_flutter/Sources/spine_flutter/spine-c/ 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$CP_OUTPUT"
    exit 1
fi

log_action "Copying spine-cpp to iOS package"
mkdir -p ios/spine_flutter/Sources/spine_flutter/spine-cpp
if CP_OUTPUT=$(cp -r ../spine-cpp/src ../spine-cpp/include ios/spine_flutter/Sources/spine_flutter/spine-cpp/ 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$CP_OUTPUT"
    exit 1
fi
rm -f ios/spine_flutter/Sources/spine_flutter/spine-cpp/src/no-cpprt.cpp

log_action "Copying spine-c to macOS package"
mkdir -p macos/spine_flutter/Sources/spine_flutter/spine-c
if CP_OUTPUT=$(cp -r ../spine-c/src ../spine-c/include macos/spine_flutter/Sources/spine_flutter/spine-c/ 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$CP_OUTPUT"
    exit 1
fi

log_action "Copying spine-cpp to macOS package"
mkdir -p macos/spine_flutter/Sources/spine_flutter/spine-cpp
if CP_OUTPUT=$(cp -r ../spine-cpp/src ../spine-cpp/include macos/spine_flutter/Sources/spine_flutter/spine-cpp/ 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$CP_OUTPUT"
    exit 1
fi
rm -f macos/spine_flutter/Sources/spine_flutter/spine-cpp/src/no-cpprt.cpp

log_action "Copying spine-c to src directory"
mkdir -p src/spine-c
if CP_OUTPUT=$(cp -r ../spine-c/src ../spine-c/include src/spine-c/ 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$CP_OUTPUT"
    exit 1
fi

log_action "Copying spine-cpp to src directory"
mkdir -p src/spine-cpp
if CP_OUTPUT=$(cp -r ../spine-cpp/src ../spine-cpp/include src/spine-cpp/ 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$CP_OUTPUT"
    exit 1
fi
rm -f src/spine-cpp/src/no-cpprt.cpp

log_summary "✓ Flutter setup completed successfully"
