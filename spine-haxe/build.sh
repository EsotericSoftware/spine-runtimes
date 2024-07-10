#!/bin/sh
set -e

if [ -z "$GITHUB_REF" ]; then
    BRANCH=$(git symbolic-ref --short -q HEAD)
else
    BRANCH=${GITHUB_REF#refs/heads/}
fi

# Get the latest commit message
COMMIT_MSG=$(git log -1 --pretty=%B)

# Public only if the commit message is in the correct format
if echo "$COMMIT_MSG" | grep -qE '^\[haxe\] Release [0-9]+\.[0-9]+\.[0-9]+$'; then
    VERSION=$(echo "$COMMIT_MSG" | sed -E 's/^\[haxe\] Release ([0-9]+\.[0-9]+\.[0-9]+)$/\1/')
    echo "Building spine-haxe $BRANCH artifacts (version $VERSION)"

    if [ ! -z "$HAXE_UPDATE_URL" ] && [ ! -z "$BRANCH" ]; then
        echo "Deploying spine-haxe $BRANCH artifacts (version $VERSION)"
        zip -r "spine-haxe-$VERSION.zip" \
            haxelib.json \
            LICENSE \
            README.md \
            spine-haxe
        curl -f -F "file=@spine-haxe-$VERSION.zip" "$HAXE_UPDATE_URL$BRANCH"
    else
        echo "Not deploying artifacts. HAXE_UPDATE_URL and/or BRANCH not set."
    fi
else
    echo "The commit is not a release - do not publish."
	echo "To release the commit has to be in the for: \"[haxe] Release x.y.z\""
fi