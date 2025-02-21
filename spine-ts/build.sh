#!/bin/sh
set -e

if [ -z "$GITHUB_REF" ];
then
	BRANCH=$(git symbolic-ref --short -q HEAD)
else
	BRANCH=${GITHUB_REF#refs/heads/}
fi

echo "Building spine-ts $BRANCH artifacts"
npm install

if ! [ -z "$TS_UPDATE_URL" ] && ! [ -z "$BRANCH" ];
then
	echo "Deploying spine-ts $BRANCH artifacts"
	zip -j spine-ts.zip \
		spine-core/dist/iife/* \
		spine-canvas/dist/iife/* \
		spine-webgl/dist/iife/* \
		spine-player/dist/iife/* \
		spine-threejs/dist/iife/* \
		spine-pixi-v7/dist/iife/* \
		spine-pixi-v8/dist/iife/* \
		spine-phaser/dist/iife/* \
		spine-core/dist/esm/* \
		spine-canvas/dist/esm/* \
		spine-webgl/dist/esm/* \
		spine-player/dist/esm/* \
		spine-threejs/dist/esm/* \
		spine-pixi-v7/dist/esm/* \
		spine-pixi-v8/dist/esm/* \
		spine-phaser/dist/esm/* \
		spine-player/css/spine-player.css
	curl -f -F "file=@spine-ts.zip" "$TS_UPDATE_URL$BRANCH"
else
	echo "Not deploying artifacts. TS_UPDATE_URL and/or BRANCH not set."
fi
