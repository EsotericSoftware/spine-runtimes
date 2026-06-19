#!/bin/bash
set -e

cd "$(dirname "$0")/.."

# Source logging utilities
source ../formatters/logging/logging.sh

# This script only packages and deploys the Construct 3 artifacts produced by scripts/build.sh.
# It intentionally does not rebuild, so npm package outputs are not changed between the
# main build step and the later npm publish step.

VERSION="${C3_RELEASE_VERSION:-${TS_RELEASE_VERSION:-}}"
C3_UPDATE_PATH="${C3_UPDATE_PATH:-}"

log_title "Spine-Construct3 Deploy"

log_action "Validating Construct3 release version"
if echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
	log_ok
else
	log_fail
	log_error_output "C3_RELEASE_VERSION or TS_RELEASE_VERSION must be set and use x.y.z, e.g. 4.3.8"
	exit 1
fi

if [ -z "$C3_UPDATE_PATH" ]; then
	C3_UPDATE_PATH=$(echo "$VERSION" | cut -d. -f1,2)
fi

log_detail "Version: $VERSION"
log_detail "C3 update path: $C3_UPDATE_PATH"

log_action "Validating deployment configuration"
if [ -n "${C3_UPDATE_URL:-}" ] && [ -n "$C3_UPDATE_PATH" ]; then
	log_ok
else
	log_fail
	log_error_output "Construct3 deployment requires C3_UPDATE_URL and C3_UPDATE_PATH."
	exit 1
fi

log_action "Validating Construct3 dist"
if VALIDATION_OUTPUT=$(C3_RELEASE_VERSION="$VERSION" node <<'NODE' 2>&1
const fs = require("fs");
const path = require("path");
const version = process.env.C3_RELEASE_VERSION;

function readJson (file) {
	let text = fs.readFileSync(file, "utf8");
	if (text.charCodeAt(0) === 0xfeff) text = text.slice(1);
	return JSON.parse(text);
}

const distDir = "spine-construct3/dist";
const addonPath = path.join(distDir, "addon.json");
if (!fs.existsSync(addonPath)) {
	console.error(`${addonPath} does not exist. Run scripts/build.sh first.`);
	process.exit(1);
}

const addon = readJson(addonPath);
let ok = true;
if (addon.version !== version) {
	console.error(`${addonPath}: version ${addon.version} does not match release version ${version}`);
	ok = false;
}

for (const file of addon["file-list"] || []) {
	if (!fs.existsSync(path.join(distDir, file))) {
		console.error(`${addonPath}: file-list entry missing from dist: ${file}`);
		ok = false;
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

rm -f spine-construct3/EsotericSoftware_SpineConstruct3.c3addon spine-construct3/spine-construct3.zip "spine-construct3/spine-construct3-$VERSION.zip"

log_action "Creating .c3addon"
pushd "spine-construct3/dist" > /dev/null
if ZIP_OUTPUT=$(zip -r ../EsotericSoftware_SpineConstruct3.c3addon ./* 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi
popd > /dev/null

log_action "Creating versioned zip: spine-construct3-$VERSION.zip"
pushd "spine-construct3" > /dev/null
if ZIP_OUTPUT=$(zip "spine-construct3-$VERSION.zip" EsotericSoftware_SpineConstruct3.c3addon 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi
popd > /dev/null

log_action "Creating latest zip: spine-construct3.zip"
pushd "spine-construct3" > /dev/null
if ZIP_OUTPUT=$(zip spine-construct3.zip EsotericSoftware_SpineConstruct3.c3addon 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi
popd > /dev/null

log_action "Uploading spine-construct3-$VERSION.zip to $C3_UPDATE_URL$C3_UPDATE_PATH"
if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3/spine-construct3-$VERSION.zip" "$C3_UPDATE_URL$C3_UPDATE_PATH" 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CURL_OUTPUT"
	exit 1
fi

log_action "Uploading spine-construct3.zip (latest) to $C3_UPDATE_URL$C3_UPDATE_PATH"
if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3/spine-construct3.zip" "$C3_UPDATE_URL$C3_UPDATE_PATH" 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CURL_OUTPUT"
	exit 1
fi

log_summary "✓ Construct3 plugin latest deployed successfully"
log_summary "✓ Construct3 plugin $VERSION deployed successfully"
