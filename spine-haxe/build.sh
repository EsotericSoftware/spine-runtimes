#!/bin/bash
set -e

cd "$(dirname "$0")"

# Source logging utilities
source ../formatters/logging/logging.sh

if [ -z "$GITHUB_REF" ]; then
    BRANCH=$(git symbolic-ref --short -q HEAD)
else
    BRANCH=${GITHUB_REF#refs/heads/}
fi

# Get the latest commit message
COMMIT_MSG=$(git log -1 --pretty=%B)

log_title "Spine-Haxe Build"
log_detail "Branch: $BRANCH"

if ! echo "$BRANCH" | grep -qE '^[0-9]+\.[0-9]+$'; then
    log_warn "Branch is not a release branch - skipping publish"
    log_detail "Release branches must be named number.number, e.g. 4.3"
    log_summary "Build skipped"
    exit 0
fi

# Publish only if the commit message is in the correct format
if echo "$COMMIT_MSG" | grep -qE '^\[haxe\] Release [0-9]+\.[0-9]+\.[0-9]+$'; then
    VERSION=$(echo "$COMMIT_MSG" | sed -E 's/^\[haxe\] Release ([0-9]+\.[0-9]+\.[0-9]+)$/\1/')
    log_detail "Version: $VERSION"

    if [ ! -z "$HAXE_UPDATE_URL" ] && [ ! -z "$BRANCH" ]; then
        log_action "Creating release package"
        if ZIP_OUTPUT=$(zip -r "spine-haxe-$VERSION.zip" \
            haxelib.json \
            LICENSE \
            README.md \
            spine-haxe 2>&1); then
            log_ok
        else
            log_fail
            log_error_output "$ZIP_OUTPUT"
            exit 1
        fi

        log_action "Uploading to $HAXE_UPDATE_URL$BRANCH"
        if CURL_OUTPUT=$(curl -f -F "file=@spine-haxe-$VERSION.zip" "$HAXE_UPDATE_URL$BRANCH" 2>&1); then
            log_ok
        else
            log_fail
            log_error_output "$CURL_OUTPUT"
            exit 1
        fi

        log_summary "✓ Build and deployment successful"
    else
        log_skip "Deployment skipped (HAXE_UPDATE_URL and/or BRANCH not set)"
        log_summary "✓ Build successful"
    fi
else
    log_warn "Commit is not a release - skipping publish"
    log_detail "To release, commit message must be: \"[haxe] Release x.y.z\""
    log_summary "Build skipped"
fi