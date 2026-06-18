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
tag="spine-haxe-$newVersion"

log_title "Spine-Haxe Publish"

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
    unreleasedChanges=$(awk '
        /^## Unreleased[[:space:]]*$/ { in_unreleased = 1; next }
        in_unreleased && /^##[[:space:]]+/ { in_unreleased = 0 }
        in_unreleased && NF { print "yes"; exit }
    ' "$CHANGELOG_FILE")

    if [ "$unreleasedChanges" = "yes" ]; then
        log_detail "CHANGELOG.md has unreleased changes."
        read -p "Move unreleased CHANGELOG.md entries to $newVersion with today's date? [y/N] " UPDATE_CHANGELOG
        case "$UPDATE_CHANGELOG" in
            [yY]|[yY][eE][sS])
                today=$(date +%Y-%m-%d)
                log_action "Updating CHANGELOG.md"
                CHANGELOG_TMP=$(mktemp "${CHANGELOG_FILE}.XXXXXX")
                if CHANGELOG_OUTPUT=$(awk -v version="$newVersion" -v release_date="$today" '
                    function flush_unreleased(   start, end, i) {
                        start = 1
                        end = line_count
                        while (start <= end && lines[start] ~ /^[[:space:]]*$/) start++
                        while (end >= start && lines[end] ~ /^[[:space:]]*$/) end--

                        print "## " version " - " release_date
                        print ""
                        for (i = start; i <= end; i++) print lines[i]
                        print ""

                        line_count = 0
                        inserted = 1
                    }

                    /^## Unreleased[[:space:]]*$/ {
                        print "## Unreleased"
                        print ""
                        in_unreleased = 1
                        next
                    }

                    in_unreleased && /^##[[:space:]]+/ {
                        flush_unreleased()
                        in_unreleased = 0
                        print
                        next
                    }

                    in_unreleased {
                        lines[++line_count] = $0
                        next
                    }

                    { print }

                    END {
                        if (in_unreleased) flush_unreleased()
                        if (!inserted) exit 1
                    }
                ' "$CHANGELOG_FILE" > "$CHANGELOG_TMP" 2>&1 && mv "$CHANGELOG_TMP" "$CHANGELOG_FILE" 2>&1); then
                    log_ok
                else
                    rm -f "$CHANGELOG_TMP"
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

log_action "Updating haxelib.json"
if SED_OUTPUT=$(sed -i '' "s/$currentVersion/$newVersion/" haxelib.json 2>&1); then
    log_ok
else
    log_fail
    log_error_output "$SED_OUTPUT"
    exit 1
fi

echo "Write Y if you want to commit, tag, and push the new version $newVersion."
echo "This will create and push tag $tag, which triggers the CI pipeline that publishes the new version on the Esoteric Software server."
echo "Do you want to proceed [y/n]?"

read answer
if [ "$answer" = "Y" ] || [ "$answer" = "y" ]; then
    log_action "Committing changes"
    if COMMIT_OUTPUT=$(git add haxelib.json CHANGELOG.md && git commit -m "[haxe] Release $newVersion" 2>&1); then
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