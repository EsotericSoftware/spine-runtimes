#!/bin/bash
# Spine-C++ Smoke Test
#
# Tests all spine-cpp build variants with spineboy example data:
# - headless-test (regular dynamic)
# - headless-test-no-cpprt (no-cpprt dynamic)
# - headless-test-static (regular static, Linux only)
# - headless-test-no-cpprt-static (no-cpprt static, Linux only)

set -e

# Change to spine-cpp root directory
cd "$(dirname "$0")/.."

# Source logging utilities
source ../formatters/logging/logging.sh

# Test configuration - spineboy example files and animation
SPINEBOY_SKEL="../examples/spineboy/export/spineboy-pro.skel"
SPINEBOY_ATLAS="../examples/spineboy/export/spineboy-pma.atlas"
SPINEBOY_ANIM="idle"

# Expected output pattern - first 10 lines of skeleton JSON data
EXPECTED_OUTPUT="=== SKELETON DATA ===
{
  \"refString\": \"<SkeletonData-spineboy-pro>\",
  \"type\": \"SkeletonData\",
  \"bones\": [{
      \"refString\": \"<BoneData-root>\",
      \"type\": \"BoneData\",
      \"index\": 0,
      \"parent\": null,
      \"length\": 0,"

log_title "Spine-C++ Test"
log_detail "Platform: $(uname)"

log_action "Building all variants"
if BUILD_OUTPUT=$(./build.sh clean release 2>&1); then
    log_ok
else
    log_fail "Build failed"
    log_detail "$BUILD_OUTPUT"
    exit 1
fi

test_count=0
pass_count=0

log_action "Testing map-test"
test_count=$((test_count + 1))
if OUTPUT=$(build/map-test 2>&1); then
    log_ok
    pass_count=$((pass_count + 1))
else
    log_fail "map-test - execution failed"
    log_detail "$OUTPUT"
    echo ""
fi

for exe in build/headless-test*; do
    if [ -f "$exe" ] && [ -x "$exe" ]; then
        exe_name=$(basename "$exe")
        log_action "Testing $exe_name"

        test_count=$((test_count + 1))

        if OUTPUT=$("$exe" $SPINEBOY_SKEL $SPINEBOY_ATLAS $SPINEBOY_ANIM 2>&1); then
            actual_output=$(echo "$OUTPUT" | head -10)

            if [ "$actual_output" = "$EXPECTED_OUTPUT" ]; then
                log_ok
                pass_count=$((pass_count + 1))
            else
                log_fail "$exe_name - output mismatch"
                log_detail "Expected:"
                log_detail "$EXPECTED_OUTPUT"
                log_detail ""
                log_detail "Actual:"
                log_detail "$actual_output"
                echo ""
            fi
        else
            log_fail "$exe_name - execution failed"
            log_detail "$OUTPUT"
            echo ""
        fi
    fi
done

if [ $pass_count -eq $test_count ] && [ $test_count -gt 0 ]; then
    log_summary "✓ All tests passed ($pass_count/$test_count)"
    exit 0
else
    log_summary "✗ Tests failed ($pass_count/$test_count)"
    exit 1
fi