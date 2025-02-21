#!/bin/sh
set -e

currentVersion=$(grep -o '"version": "[^"]*' haxelib.json | grep -o '[^"]*$')

major=$(echo "$currentVersion" | cut -d. -f1)
minor=$(echo "$currentVersion" | cut -d. -f2)
patch=$(echo "$currentVersion" | cut -d. -f3)
newPatch=$((patch + 1))
newVersion="$major.$minor.$newPatch"

echo "current version: $currentVersion"
echo "new version: $newVersion"

sed -i '' "s/$currentVersion/$newVersion/" haxelib.json

echo "Write Y if you want to commit and push the new version $newVersion."
echo "This will trigger a pipeline that will publish the new version on esoteric software server."
echo "Do you want to proceed [y/n]?"

read answer
if [ "$answer" = "Y" ] || [ "$answer" = "y" ]; then
    git add haxelib.json
    git commit -m "[haxe] Release $newVersion"
    git push origin 4.2
    echo "Changes committed and pushed."
else
    echo "Commit and push cancelled, but haxelib.json version updated."
fi