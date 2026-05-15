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

currentVersion=$(grep -o '"version": "[^"]*' haxelib.json | grep -o '[^"]*$')

major=$(echo "$currentVersion" | cut -d. -f1)
minor=$(echo "$currentVersion" | cut -d. -f2)
patch=$(echo "$currentVersion" | cut -d. -f3)
newPatch=$((patch + 1))
newVersion="$major.$minor.$newPatch"

log_title "Spine-Haxe Publish"

log_detail "Branch: $BRANCH"
log_detail "Current version: $currentVersion"
log_detail "New version: $newVersion"

log_action "Updating haxelib.json"
if SED_OUTPUT=$(sed -i '' "s/$currentVersion/$newVersion/" haxelib.json 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$SED_OUTPUT"
    exit 1
fi

echo "Write Y if you want to commit and push the new version $newVersion."
echo "This will trigger a pipeline that will publish the new version on esoteric software server."
echo "Do you want to proceed [y/n]?"

read answer
if [ "$answer" = "Y" ] || [ "$answer" = "y" ]; then
    log_action "Committing changes"
    if COMMIT_OUTPUT=$(git add haxelib.json && git commit -m "[haxe] Release $newVersion" 2>&1); then
        log_ok
    else
        log_fail
        log_error_output "$COMMIT_OUTPUT"
        exit 1
    fi
    
    log_action "Pushing to origin"
    if PUSH_OUTPUT=$(git push origin "$BRANCH" 2>&1); then
        log_ok
        log_summary "✓ Version $newVersion published successfully"
    else
        log_fail
        log_error_output "$PUSH_OUTPUT"
        exit 1
    fi
else
    log_action "Publishing version"
    log_skip
    log_summary "Version updated locally only"
fi