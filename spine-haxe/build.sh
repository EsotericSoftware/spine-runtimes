#!/bin/bash
set -e

cd "$(dirname "$0")"

# Source logging utilities
source ../formatters/logging/logging.sh

TAG_PREFIX="spine-haxe-"
TAG=""
VERSION="${HAXE_RELEASE_VERSION:-}"

if [ "${GITHUB_REF_TYPE:-}" = "tag" ]; then
    TAG="${GITHUB_REF_NAME:-}"
elif echo "${GITHUB_REF:-}" | grep -qE '^refs/tags/'; then
    TAG=${GITHUB_REF#refs/tags/}
fi

log_title "Spine-Haxe Build"

if [ -n "$TAG" ]; then
    log_detail "Tag: $TAG"
    log_action "Validating release tag"
    if echo "$TAG" | grep -qE "^${TAG_PREFIX}[0-9]+\.[0-9]+\.[0-9]+$"; then
        log_ok
    else
        log_fail
        log_error_output "Release tags must use the form ${TAG_PREFIX}x.y.z, e.g. ${TAG_PREFIX}4.3.2"
        exit 1
    fi
    VERSION=${TAG#$TAG_PREFIX}
elif [ -n "$VERSION" ]; then
    log_detail "Manual release version: $VERSION"
    log_action "Validating manual release version"
    if echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
        log_ok
    else
        log_fail
        log_error_output "Manual release versions must use the form x.y.z, e.g. 4.3.2"
        exit 1
    fi
else
    log_warn "No release tag detected - skipping publish"
    log_detail "To release, push a tag named ${TAG_PREFIX}x.y.z"
    log_summary "Build skipped"
    exit 0
fi

PACKAGE_VERSION=$(sed -nE 's/.*"version": "([^"]+)".*/\1/p' haxelib.json | head -1)
log_action "Validating haxelib.json version"
if [ "$PACKAGE_VERSION" = "$VERSION" ]; then
    log_ok
else
    log_fail
    log_error_output "Release version ($VERSION) does not match haxelib.json ($PACKAGE_VERSION)."
    exit 1
fi

RELEASE_LINE=$(echo "$VERSION" | cut -d. -f1,2)

log_detail "Version: $VERSION"
log_detail "Release line: $RELEASE_LINE"

if [ -n "${HAXE_UPDATE_URL:-}" ]; then
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

    log_action "Uploading to $HAXE_UPDATE_URL$RELEASE_LINE"
    if CURL_OUTPUT=$(curl -f -F "file=@spine-haxe-$VERSION.zip" "$HAXE_UPDATE_URL$RELEASE_LINE" 2>&1); then
        log_ok
    else
        log_fail
        log_error_output "$CURL_OUTPUT"
        exit 1
    fi

    log_summary "✓ Build and deployment successful"
else
    log_action "Deployment"
    log_skip
    log_detail "Deployment skipped (HAXE_UPDATE_URL not set)"
    log_summary "✓ Build successful"
fi
