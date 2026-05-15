#!/bin/bash
set -e

cd "$(dirname "$0")"

# Source logging utilities
source ../formatters/logging/logging.sh

if [ -z "$GITHUB_REF" ];
then
	BRANCH=$(git symbolic-ref --short -q HEAD)
else
	BRANCH=${GITHUB_REF#refs/heads/}
fi

log_title "Spine-TS Build"
log_detail "Branch: $BRANCH"

if ! echo "$BRANCH" | grep -qE '^[0-9]+\.[0-9]+$'; then
	log_warn "Branch is not a release branch - skipping publish"
	log_detail "Release branches must be named number.number, e.g. 4.3"
	log_summary "Build skipped"
	exit 0
fi

log_action "Installing dependencies"
if npm install > /tmp/npm-install.log 2>&1; then
	log_ok
else
	log_fail
	log_error_output "$(cat /tmp/npm-install.log)"
	exit 1
fi

if ! [ -z "$TS_UPDATE_URL" ] && ! [ -z "$BRANCH" ];
then
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
	
	log_action "Uploading to $TS_UPDATE_URL$BRANCH"
	if CURL_OUTPUT=$(curl -f -F "file=@spine-ts.zip" "$TS_UPDATE_URL$BRANCH" 2>&1); then
		log_ok
	else
		log_fail
		log_error_output "$CURL_OUTPUT"
		exit 1
	fi
	
	log_summary "✓ Build and deployment successful"
else
	log_skip "Deployment skipped (TS_UPDATE_URL and/or BRANCH not set)"
	log_summary "✓ Build successful"
fi
