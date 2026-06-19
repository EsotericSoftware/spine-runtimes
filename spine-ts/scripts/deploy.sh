#!/bin/bash
set -e

cd "$(dirname "$0")/.."

# Source logging utilities
source ../formatters/logging/logging.sh

VERSION="${TS_RELEASE_VERSION:-}"
TS_UPDATE_PATH="${TS_UPDATE_PATH:-}"

log_title "Spine-TS Deploy"

log_action "Validating release version"
if echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
	log_ok
else
	log_fail
	log_error_output "TS_RELEASE_VERSION must be set and use x.y.z, e.g. 4.3.8"
	exit 1
fi

if [ -z "$TS_UPDATE_PATH" ]; then
	TS_UPDATE_PATH=$(echo "$VERSION" | cut -d. -f1,2)
fi

log_detail "Version: $VERSION"
log_detail "TS update path: $TS_UPDATE_PATH"

log_action "Validating deployment configuration"
if [ -n "${TS_UPDATE_URL:-}" ] && [ -n "$TS_UPDATE_PATH" ]; then
	log_ok
else
	log_fail
	log_error_output "spine-ts deployment requires TS_UPDATE_URL and TS_UPDATE_PATH."
	exit 1
fi

rm -f spine-ts.zip

log_action "Creating artifacts zip"
if ZIP_OUTPUT=$(zip -j spine-ts.zip \
	spine-core/dist/iife/* \
	spine-canvas/dist/iife/* \
	spine-webgl/dist/iife/* \
	spine-player/dist/iife/* \
	spine-threejs/dist/iife/* \
	spine-pixi-v7/dist/iife/* \
	spine-pixi-v8/dist/iife/* \
	spine-phaser-v3/dist/iife/* \
	spine-phaser-v4/dist/iife/* \
	spine-webcomponents/dist/iife/* \
	spine-core/dist/esm/* \
	spine-canvas/dist/esm/* \
	spine-webgl/dist/esm/* \
	spine-player/dist/esm/* \
	spine-threejs/dist/esm/* \
	spine-pixi-v7/dist/esm/* \
	spine-pixi-v8/dist/esm/* \
	spine-phaser-v3/dist/esm/* \
	spine-phaser-v4/dist/esm/* \
	spine-webcomponents/dist/esm/* \
	spine-player/css/spine-player.css 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi

log_action "Uploading spine-ts.zip to $TS_UPDATE_URL$TS_UPDATE_PATH"
if CURL_OUTPUT=$(curl -f -F "file=@spine-ts.zip" "$TS_UPDATE_URL$TS_UPDATE_PATH" 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CURL_OUTPUT"
	exit 1
fi

log_summary "✓ spine-ts artifacts deployed successfully"
