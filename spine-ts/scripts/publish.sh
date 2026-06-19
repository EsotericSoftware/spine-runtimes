#!/bin/bash
set -e
cd "$(dirname "$0")/.."

# Source logging utilities
source ../formatters/logging/logging.sh

BRANCH=$(git symbolic-ref --short -q HEAD)
if ! echo "$BRANCH" | grep -qE '^[0-9]+\.[0-9]+$'; then
    echo "Error: scripts/publish.sh can only be run from release branches named number.number, e.g. 4.3. Current branch: $BRANCH"
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
tag="spine-ts-$newVersion"

log_detail "Branch: $BRANCH"
log_detail "Current version: $currentVersion"
log_detail "New version: $newVersion"
log_detail "Release tag: $tag"

log_action "Validating release tag"
if git rev-parse -q --verify "refs/tags/$tag" >/dev/null; then
    log_fail
    log_error_output "Tag $tag already exists."
    exit 1
else
    log_ok
fi

CHANGELOG_FILE="CHANGELOG.md"
if [ -f "$CHANGELOG_FILE" ]; then
    unreleasedChanges=$(node <<'NODE'
const fs = require("fs");
const text = fs.readFileSync("CHANGELOG.md", "utf8");
const match = text.match(/^## Unreleased\s*\n(?<body>.*?)(?=^##\s+)/ms);
if (match?.groups?.body.trim()) console.log("yes");
NODE
)

    if [ "$unreleasedChanges" = "yes" ]; then
        log_detail "CHANGELOG.md has unreleased changes."
        read -p "Move unreleased CHANGELOG.md entries to $newVersion with today's date? [y/N] " UPDATE_CHANGELOG
        case "$UPDATE_CHANGELOG" in
            [yY]|[yY][eE][sS])
                today=$(date +%Y-%m-%d)
                log_action "Updating CHANGELOG.md"
                if CHANGELOG_OUTPUT=$(NEW_VERSION="$newVersion" RELEASE_DATE="$today" node <<'NODE' 2>&1
const fs = require("fs");
const path = "CHANGELOG.md";
const text = fs.readFileSync(path, "utf8");
const version = process.env.NEW_VERSION;
const date = process.env.RELEASE_DATE;
const match = /^## Unreleased\s*\n(?<body>.*?)(?=^##\s+)/ms.exec(text);
if (!match) {
    console.error("Could not find an Unreleased section in CHANGELOG.md");
    process.exit(1);
}
const body = match.groups.body.trim();
if (!body) process.exit(0);
const replacement = `## Unreleased\n\n## ${version} - ${date}\n\n${body}\n\n`;
fs.writeFileSync(path, text.slice(0, match.index) + replacement + text.slice(match.index + match[0].length));
NODE
                ); then
                    log_ok
                else
                    log_fail
                    log_error_output "$CHANGELOG_OUTPUT"
                    exit 1
                fi
                ;;
            *)
                log_detail "Leaving unreleased CHANGELOG.md entries unchanged."
                ;;
        esac
    else
        log_detail "CHANGELOG.md has no unreleased changes."
    fi
else
    log_warn "CHANGELOG.md not found; skipping changelog update."
fi

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
    if UPDATE_OUTPUT=$(PACKAGE_PATH="$package" CURRENT_VERSION="$currentVersion" NEW_VERSION="$newVersion" node <<'NODE' 2>&1
const fs = require("fs");
const path = process.env.PACKAGE_PATH;
const currentVersion = process.env.CURRENT_VERSION;
const newVersion = process.env.NEW_VERSION;
const text = fs.readFileSync(path, "utf8");
if (!text.includes(currentVersion)) {
    console.error(`${currentVersion} not found in ${path}`);
    process.exit(1);
}
fs.writeFileSync(path, text.split(currentVersion).join(newVersion));
NODE
    ); then
        log_ok
    else
        log_fail
        log_error_output "$UPDATE_OUTPUT"
        exit 1
    fi
done

log_action "Cleaning @esotericsoftware modules"
if RM_OUTPUT=$(rm -rf node_modules/@esotericsoftware 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$RM_OUTPUT"
    exit 1
fi

log_action "Installing workspace dependencies"
if NPM_OUTPUT=$(npm install --workspaces 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$NPM_OUTPUT"
    exit 1
fi

echo "Write Y if you want to commit, tag, and push the new version $newVersion."
echo "This will create and push tag $tag, which triggers the CI pipeline that uploads the web and Construct 3 artifacts, then publishes the npm packages."
echo "Do you want to proceed [y/n]?"

read answer
if [ "$answer" = "Y" ] || [ "$answer" = "y" ]; then
    log_action "Committing changes"
    if COMMIT_OUTPUT=$(git add CHANGELOG.md package-lock.json "${packages[@]}" && git commit -m "[ts] Release $newVersion" 2>&1); then
        log_ok
    else
        log_fail
        log_error_output "$COMMIT_OUTPUT"
        exit 1
    fi

    log_action "Creating tag $tag"
    if TAG_OUTPUT=$(git tag "$tag" 2>&1); then
        log_ok
    else
        log_fail
        log_error_output "$TAG_OUTPUT"
        exit 1
    fi

    log_action "Pushing release branch and tag to origin"
    if PUSH_OUTPUT=$(git push --atomic origin "$BRANCH" "$tag" 2>&1); then
        log_ok
        log_summary "✓ Version $newVersion tagged and pushed successfully"
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
