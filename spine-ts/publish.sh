#!/bin/sh
set -e

if [ ! "$#" -eq 2 ]; then
	echo "Usage: ./publish.sh <last-version> <new-version>"
	exit	
else
	lastVersion=${1%/}
	newVersion=${2%/}
	echo "last version: $lastVersion"
	echo "new version: $newVersion"
fi

sed -i '' "s/$lastVersion/$newVersion/" package.json
sed -i '' "s/$lastVersion/$newVersion/" spine-canvas/package.json
sed -i '' "s/$lastVersion/$newVersion/" spine-core/package.json
sed -i '' "s/$lastVersion/$newVersion/" spine-player/package.json
sed -i '' "s/$lastVersion/$newVersion/" spine-threejs/package.json
sed -i '' "s/$lastVersion/$newVersion/" spine-webgl/package.json

rm -rf node_modules
rm package-lock.json
npm install
npm publish --access public --workspaces