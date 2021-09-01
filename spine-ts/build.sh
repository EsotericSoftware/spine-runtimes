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
		spine-player/css/spine-player.css \
		spine-player/example/external/*
	curl -f -F "file=@spine-ts.zip" "$TS_UPDATE_URL$BRANCH"
else
	echo "Not deploying artifacts. TS_UPDATE_URL and/or BRANCH not set."
fi
