#!/bin/bash
set -e

cd "$(dirname "$0")"

# Source logging utilities
source ../formatters/logging/logging.sh

TAG_PREFIX="spine-ts-"
TAG=""
VERSION="${TS_RELEASE_VERSION:-}"

if [ "${GITHUB_REF_TYPE:-}" = "tag" ]; then
	TAG="${GITHUB_REF_NAME:-}"
elif echo "${GITHUB_REF:-}" | grep -qE '^refs/tags/'; then
	TAG=${GITHUB_REF#refs/tags/}
fi

log_title "Spine-TS Build"

if [ -n "$TAG" ]; then
	log_detail "Tag: $TAG"
	log_action "Validating release tag"
	if echo "$TAG" | grep -qE "^${TAG_PREFIX}[0-9]+\.[0-9]+\.[0-9]+$"; then
		log_ok
	else
		log_fail
		log_error_output "Release tags must use the form ${TAG_PREFIX}x.y.z, e.g. ${TAG_PREFIX}4.3.8"
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
		log_error_output "Manual release versions must use the form x.y.z, e.g. 4.3.8"
		exit 1
	fi
else
	log_warn "No release tag detected - skipping publish"
	log_detail "To release, push a tag named ${TAG_PREFIX}x.y.z"
	log_summary "Build skipped"
	exit 0
fi

log_action "Validating package versions"
if VALIDATION_OUTPUT=$(RELEASE_VERSION="$VERSION" node <<'NODE' 2>&1
const fs = require("fs");
const version = process.env.RELEASE_VERSION;
const packages = [
	"package.json",
	"spine-canvas/package.json",
	"spine-canvaskit/package.json",
	"spine-core/package.json",
	"spine-phaser-v3/package.json",
	"spine-phaser-v4/package.json",
	"spine-pixi-v7/package.json",
	"spine-pixi-v8/package.json",
	"spine-player/package.json",
	"spine-threejs/package.json",
	"spine-webcomponents/package.json",
	"spine-webgl/package.json",
];
let ok = true;
for (const file of packages) {
	const pkg = JSON.parse(fs.readFileSync(file, "utf8"));
	if (pkg.version !== version) {
		console.error(`${file}: version ${pkg.version} does not match release version ${version}`);
		ok = false;
	}
	for (const section of ["dependencies", "peerDependencies", "devDependencies", "optionalDependencies"]) {
		const deps = pkg[section] || {};
		for (const [name, range] of Object.entries(deps)) {
			if (name.startsWith("@esotericsoftware/spine-") && range !== version) {
				console.error(`${file}: ${section}.${name} is ${range}, expected ${version}`);
				ok = false;
			}
		}
	}
}
process.exit(ok ? 0 : 1);
NODE
); then
	log_ok
else
	log_fail
	log_error_output "$VALIDATION_OUTPUT"
	exit 1
fi

RELEASE_LINE=$(echo "$VERSION" | cut -d. -f1,2)

log_detail "Version: $VERSION"
log_detail "Release line: $RELEASE_LINE"

log_action "Installing dependencies"
if NPM_OUTPUT=$(npm ci 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$NPM_OUTPUT"
	exit 1
fi

log_action "Building packages"
if NPM_OUTPUT=$(npm run build 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$NPM_OUTPUT"
	exit 1
fi

if [ -n "${TS_UPDATE_URL:-}" ]; then
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

	log_action "Uploading to $TS_UPDATE_URL$RELEASE_LINE"
	if CURL_OUTPUT=$(curl -f -F "file=@spine-ts.zip" "$TS_UPDATE_URL$RELEASE_LINE" 2>&1); then
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
	log_detail "Deployment skipped (TS_UPDATE_URL not set)"
	log_summary "✓ Build successful"
fi
