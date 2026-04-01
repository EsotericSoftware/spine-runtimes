#!/bin/bash

set -e

# Get to the script's directory
cd "$(dirname "$0")"

# Source logging utilities
source ../formatters/logging/logging.sh

log_title "spine-swift bindings generation"

# Install dependencies if needed
if [ ! -d "codegen/node_modules" ]; then
    log_action "Installing codegen dependencies"
    if (cd codegen && npm install > /dev/null 2>&1); then
        log_ok
    else
        log_fail
        exit 1
    fi
fi

# Generating spine-c bindings
log_action "Generating spine-c bindings"
if LOG=$(cd ../spine-c && ./build.sh codegen 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$LOG"
    exit 1
fi

# Run the Swift codegen
log_action "Generating Swift bindings"
if LOG=$(cd codegen && npm run generate 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$LOG"
    exit 1
fi

# Clean SwiftPM build state so removed generated sources don't remain in stale source lists.
log_action "Cleaning Swift build directories"
rm -rf ../.build test/.build
log_ok

# Build and run tests
log_action "Building and running tests"
if LOG=$(cd test && ./build.sh 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$LOG"
    exit 1
fi

log_summary "✓ Swift bindings generated successfully"