#!/bin/bash
set -e

cd "$(dirname "$0")/.."

# Source logging utilities
source ../formatters/logging/logging.sh

VERSION="${TS_RELEASE_VERSION:-}"

log_title "Spine-TS Build"

log_action "Validating release version"
if echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
	log_ok
else
	log_fail
	log_error_output "TS_RELEASE_VERSION must be set and use x.y.z, e.g. 4.3.8"
	exit 1
fi

log_detail "Version: $VERSION"

log_action "Validating package versions"
if VALIDATION_OUTPUT=$(RELEASE_VERSION="$VERSION" node <<'NODE' 2>&1
const fs = require("fs");
const version = process.env.RELEASE_VERSION;
const publicPackages = [
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
const construct3Packages = [
	"spine-construct3/package.json",
	"spine-construct3/spine-construct3-lib/package.json",
];
const construct3Addon = "spine-construct3/src/addon.json";

function readJson (file) {
	let text = fs.readFileSync(file, "utf8");
	if (text.charCodeAt(0) === 0xfeff) text = text.slice(1);
	return JSON.parse(text);
}

let ok = true;
function validatePackage (file, validateInternalDependencyVersions) {
	const pkg = readJson(file);
	if (pkg.version !== version) {
		console.error(`${file}: version ${pkg.version} does not match release version ${version}`);
		ok = false;
	}
	if (!validateInternalDependencyVersions) return;

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

for (const file of publicPackages) validatePackage(file, true);
for (const file of construct3Packages) validatePackage(file, false);

const addon = readJson(construct3Addon);
if (addon.version !== version) {
	console.error(`${construct3Addon}: version ${addon.version} does not match release version ${version}`);
	ok = false;
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

log_summary "✓ Build successful"
