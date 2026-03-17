#!/bin/bash

set -euo pipefail

# Script to generate all Spine runtime bindings
# This script regenerates bindings for C, Flutter, and iOS

# Get the directory containing this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

rm -rf "$SCRIPT_DIR/spine-c/codegen/spine-cpp-types.json"
rm -rf "$SCRIPT_DIR/spine-c/codegen/node_modules"
rm -rf "$SCRIPT_DIR/spine-flutter/codegen/node_modules"
rm -rf "$SCRIPT_DIR/spine-ios/codegen/node_modules"

# Source logging utilities
source "$SCRIPT_DIR/formatters/logging/logging.sh"

log_title "Generating all Spine runtime bindings"

# Generate C bindings and test compilation
log_info "Generating C bindings"
(
	cd "$SCRIPT_DIR/spine-c"
	./build.sh codegen
)

# Generate Flutter bindings
log_info "Generating Dart bindings"
(
	cd "$SCRIPT_DIR/spine-flutter"
	./generate-bindings.sh
)

# Generate iOS Swift bindings
log_info "Generating Swift bindings for iOS"
(
	cd "$SCRIPT_DIR/spine-ios"
	./generate-bindings.sh
)

log_summary "All bindings generated successfully"
