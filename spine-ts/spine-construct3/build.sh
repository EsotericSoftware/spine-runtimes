#!/bin/bash
set -e

cd "$(dirname "$0")"

# Source logging utilities
source ../../formatters/logging/logging.sh

VERSION="${C3_RELEASE_VERSION:-}"
C3_UPDATE_PATH="${C3_UPDATE_PATH:-}"
C3_REQUIRE_UPLOAD="${C3_REQUIRE_UPLOAD:-}"
C3_BUILD_RUNTIME="${C3_BUILD_RUNTIME:-}"

log_title "Spine-Construct3 Package"

if [ -n "$VERSION" ]; then
	log_action "Validating Construct3 release version"
	if echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
		log_ok
		log_detail "Version: $VERSION"
	else
		log_fail
		log_error_output "Construct3 release versions must use the form x.y.z, e.g. 4.3.8"
		exit 1
	fi
fi

if [ -z "$C3_UPDATE_PATH" ]; then
	if [ -n "$VERSION" ]; then
		C3_UPDATE_PATH=$(echo "$VERSION" | cut -d. -f1,2)
	else
		C3_UPDATE_PATH=$(git symbolic-ref --short -q HEAD || true)
	fi
fi

if [ -n "$C3_UPDATE_PATH" ]; then
	log_detail "C3 update path: $C3_UPDATE_PATH"
fi

if [ "$C3_BUILD_RUNTIME" = "true" ] || [ "$C3_BUILD_RUNTIME" = "1" ] || [ ! -f "dist/addon.json" ]; then
	if [ ! -d "../node_modules" ]; then
		log_action "Installing dependencies"
		pushd ".." > /dev/null
		if NPM_OUTPUT=$(npm install 2>&1); then
			log_ok
		else
			log_fail
			log_error_output "$NPM_OUTPUT"
			exit 1
		fi
		popd > /dev/null
	fi

	log_action "Building Construct3 plugin"
	pushd ".." > /dev/null
	if BUILD_OUTPUT=$(npm run build:construct3 2>&1); then
		log_ok
	else
		log_fail
		log_error_output "$BUILD_OUTPUT"
		exit 1
	fi
	popd > /dev/null
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

const addonPath = "dist/addon.json";
if (!fs.existsSync(addonPath)) {
	console.error(`${addonPath} does not exist. Run npm run build:construct3 first.`);
	process.exit(1);
}

const addon = readJson(addonPath);
let ok = true;
if (version && addon.version !== version) {
	console.error(`${addonPath}: version ${addon.version} does not match release version ${version}`);
	ok = false;
}

for (const file of addon["file-list"] || []) {
	if (!fs.existsSync(path.join("dist", file))) {
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

rm -f EsotericSoftware_SpineConstruct3.c3addon spine-construct3.zip
if [ -n "$VERSION" ]; then
	rm -f "spine-construct3-$VERSION.zip"
fi

log_action "Creating .c3addon"
pushd "dist" > /dev/null
if ZIP_OUTPUT=$(zip -r ../EsotericSoftware_SpineConstruct3.c3addon ./* 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi
popd > /dev/null

if [ -n "$VERSION" ]; then
	log_action "Creating versioned zip: spine-construct3-$VERSION.zip"
	if ZIP_OUTPUT=$(zip "spine-construct3-$VERSION.zip" EsotericSoftware_SpineConstruct3.c3addon 2>&1); then
		log_ok
	else
		log_fail
		log_error_output "$ZIP_OUTPUT"
		exit 1
	fi
fi

log_action "Creating latest zip: spine-construct3.zip"
if ZIP_OUTPUT=$(zip spine-construct3.zip EsotericSoftware_SpineConstruct3.c3addon 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi

if [ -z "$C3_UPDATE_URL" ] || [ -z "$C3_UPDATE_PATH" ]; then
	if [ "$C3_REQUIRE_UPLOAD" = "true" ] || [ "$C3_REQUIRE_UPLOAD" = "1" ]; then
		log_action "Validating Construct3 deployment configuration"
		log_fail
		log_error_output "Construct3 upload requires C3_UPDATE_URL and C3_UPDATE_PATH."
		exit 1
	fi

	log_action "Construct3 deployment"
	log_skip
	log_detail "Deployment skipped (C3_UPDATE_URL and/or C3_UPDATE_PATH not set)"
	log_summary "✓ Construct3 plugin packaged successfully"
	exit 0
fi

if [ -n "$VERSION" ]; then
	log_action "Uploading spine-construct3-$VERSION.zip to $C3_UPDATE_URL$C3_UPDATE_PATH"
	if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3-$VERSION.zip" "$C3_UPDATE_URL$C3_UPDATE_PATH" 2>&1); then
		log_ok
	else
		log_fail
		log_error_output "$CURL_OUTPUT"
		exit 1
	fi
fi

log_action "Uploading spine-construct3.zip (latest) to $C3_UPDATE_URL$C3_UPDATE_PATH"
if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3.zip" "$C3_UPDATE_URL$C3_UPDATE_PATH" 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CURL_OUTPUT"
	exit 1
fi

log_summary "✓ Construct3 plugin latest deployed successfully"
if [ -n "$VERSION" ]; then
	log_summary "✓ Construct3 plugin $VERSION deployed successfully"
fi
