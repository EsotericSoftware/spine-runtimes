#!/bin/bash
set -e
cd "$(dirname "$0")"

# Source logging utilities
source ../formatters/logging/logging.sh

BRANCH=$(git symbolic-ref --short -q HEAD)
if ! echo "$BRANCH" | grep -qE '^[0-9]+\.[0-9]+$'; then
    echo "Error: publish.sh can only be run from release branches named number.number, e.g. 4.3. Current branch: $BRANCH"
    exit 1
fi

log_title "Spine TypeScript Publisher"

# Get current version
currentVersion=$(grep -o '"version": "[^"]*' package.json | grep -o '[^"]*$')
major=$(echo "$currentVersion" | cut -d. -f1)
minor=$(echo "$currentVersion" | cut -d. -f2)
patch=$(echo "$currentVersion" | cut -d. -f3)
newPatch=$((patch + 1))
newVersion="$major.$minor.$newPatch"

log_detail "Branch: $BRANCH"
log_detail "Current version: $currentVersion"
log_detail "New version: $newVersion"
packages=(
    "package.json"
    "spine-canvas/package.json"
    "spine-canvaskit/package.json"
    "spine-core/package.json"
    "spine-phaser-v3/package.json"
    "spine-phaser-v4/package.json"
    "spine-pixi-v7/package.json"
    "spine-pixi-v8/package.json"
    "spine-player/package.json"
    "spine-threejs/package.json"
    "spine-webgl/package.json"
    "spine-webcomponents/package.json"
    "spine-construct3/package.json"
    "spine-construct3/spine-construct3-lib/package.json"
    "spine-construct3/src/addon.json"
)

for package in "${packages[@]}"; do
    log_action "Updating $package"
    if SED_OUTPUT=$(sed -i '' "s/$currentVersion/$newVersion/" "$package" 2>&1); then
        log_ok
    else
        log_fail
        log_error_output "$SED_OUTPUT"
        exit 1
    fi
done

log_action "Removing package-lock.json"
if RM_OUTPUT=$(rm package-lock.json 2>&1); then
    log_ok
else
    log_warn
fi

log_action "Cleaning @esotericsoftware modules"
if RM_OUTPUT=$(rm -rf node_modules/@esotericsoftware 2>&1); then
    log_ok
else
    log_warn
fi

log_action "Installing workspace dependencies"
if NPM_OUTPUT=$(npm install --workspaces 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$NPM_OUTPUT"
    exit 1
fi

log_action "Publishing all workspaces"
if NPM_OUTPUT=$(npm publish --access public --workspaces 2>&1); then
    log_ok
    log_summary "✓ TypeScript packages published successfully with version $newVersion"
else
    log_fail
    log_error_output "$NPM_OUTPUT"
    exit 1
fi