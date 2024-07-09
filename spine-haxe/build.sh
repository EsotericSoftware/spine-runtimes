#!/bin/sh
set -e

if [ -z "$GITHUB_REF" ];
then
	BRANCH=$(git symbolic-ref --short -q HEAD)
else
	BRANCH=${GITHUB_REF#refs/heads/}
fi

echo "Building spine-haxe $BRANCH artifacts"

if ! [ -z "$HAXE_UPDATE_URL" ] && ! [ -z "$BRANCH" ];
then
	echo "Deploying spine-haxe $BRANCH artifacts"
	zip -r spine-haxe.zip \
		haxelib.json \
		LICENSE \
		README.md \
		spine-haxe
	curl -f -F "file=@spine-haxe.zip" "$HAXE_UPDATE_URL$BRANCH"
else
	echo "Not deploying artifacts. HAXE_UPDATE_URL and/or BRANCH not set."
fi
