#!/bin/sh
set -e

if [ -z "$GITHUB_REF" ];
then
    BRANCH=$(git symbolic-ref --short -q HEAD)
else
    BRANCH=${GITHUB_REF#refs/heads/}
fi

echo "Building spine-ts $BRANCH artifacts"
tsc -p tsconfig.json
tsc -p tsconfig.core.json
tsc -p tsconfig.webgl.json
tsc -p tsconfig.canvas.json
tsc -p tsconfig.threejs.json
tsc -p tsconfig.player.json
ls build/*.js build/*.ts | awk '{print "unexpand -t 4 ", $0, " > /tmp/e; mv /tmp/e ", $0}' | sh

if ! [ -z "$TS_UPDATE_URL" ] && ! [ -z "$BRANCH" ];
then
    echo "Deploying spine-ts $BRANCH artifacts"
    zip -j spine-ts.zip build/* player/css/spine-player.css player/example/external/*
    curl -F "file=@spine-ts.zip" "$TS_UPDATE_URL$BRANCH"
else
    echo "Not deploying artifacts. TS_UPDATE_URL and/or BRANCH not set."
fi