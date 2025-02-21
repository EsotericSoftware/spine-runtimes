#!/bin/sh
set -e

currentVersion=$(grep -o '"version": "[^"]*' package.json | grep -o '[^"]*$')
major=$(echo "$currentVersion" | cut -d. -f1)
minor=$(echo "$currentVersion" | cut -d. -f2)
patch=$(echo "$currentVersion" | cut -d. -f3)
newPatch=$((patch + 1))
newVersion="$major.$minor.$newPatch"

echo "current version: $currentVersion"
echo "new version: $newVersion"

sed -i '' "s/$currentVersion/$newVersion/" package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-canvas/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-canvaskit/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-core/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-phaser/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-pixi-v7/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-pixi-v8/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-player/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-threejs/package.json
sed -i '' "s/$currentVersion/$newVersion/" spine-webgl/package.json

rm package-lock.json
rm -rf node_modules/@esotericsoftware
npm install --workspaces
npm publish --access public --workspaces