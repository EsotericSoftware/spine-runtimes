#!/bin/sh
set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
cd $SCRIPT_DIR

# On macOS, we need gsed for the -i switch to work. Check it's available
# and error out otherwise.
sed="sed"
if [[ $OSTYPE == 'darwin'* ]]; then
  if [ ! -f "/opt/homebrew/bin/gsed" ]; then
	echo "macOS sed detected. Please install GNU sed via brew install gnu-sed"
	exit -1
  fi
  sed="/opt/homebrew/bin/gsed"
fi

ROOT=$SCRIPT_DIR/../..
echo "Spine Runtimes path: $ROOT"
echo "Copying assets to runtimes..."
echo ""

echo "spine-libgdx"
rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"*
cp -f ../goblins/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"
cp -f ../goblins/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"
cp -f ../goblins/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"*
cp -f ../raptor/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"
cp -f ../raptor/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"
cp -f ../raptor/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"*
cp -f ../spineboy/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"
cp -r ../spineboy/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"
cp -r ../spineboy/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/dragon/"*
mkdir -p "$ROOT/spine-libgdx/spine-libgdx-tests/assets/dragon/"
cp -f ../dragon/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/dragon/"
cp -f ../dragon/export/dragon-pma.atlas "$ROOT/spine-libgdx/spine-libgdx-tests/assets/dragon/"
cp -f ../dragon/export/dragon-pma*.png "$ROOT/spine-libgdx/spine-libgdx-tests/assets/dragon/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"*
cp -f ../coin/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"
cp -f ../coin/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"
cp -f ../coin/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"*
cp -f ../mix-and-match/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"
cp -f ../mix-and-match/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"
cp -f ../mix-and-match/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"

echo "spine-android"
rm -f "$ROOT/spine-android/app/src/main/assets/"*
cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-android/app/src/main/assets/"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-android/app/src/main/assets"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-android/app/src/main/assets"

cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-android/app/src/main/assets/"
cp -f ../dragon/export/dragon.atlas "$ROOT/spine-android/app/src/main/assets"
cp -f ../dragon/export/dragon.png "$ROOT/spine-android/app/src/main/assets"
cp -f ../dragon/export/dragon_*.png "$ROOT/spine-android/app/src/main/assets"

cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-android/app/src/main/assets/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-android/app/src/main/assets/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-android/app/src/main/assets/"

cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-android/app/src/main/assets/"
cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-android/app/src/main/assets/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-android/app/src/main/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-android/app/src/main/assets/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/sack/"*
mkdir -p "$ROOT/spine-libgdx/spine-libgdx-tests/assets/sack/"
cp -f ../7-anticipation/export/sack-pro.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/sack/"
cp -f ../7-anticipation/export/sack-pro.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/sack/"
cp -f ../7-anticipation/export/7-anticipation-pma.atlas "$ROOT/spine-libgdx/spine-libgdx-tests/assets/sack/sack-pma.atlas"
$sed -i 's/7-anticipation-pma.png/sack-pma.png/g' "$ROOT/spine-libgdx/spine-libgdx-tests/assets/sack/sack-pma.atlas"
cp -f ../7-anticipation/export/7-anticipation-pma.png "$ROOT/spine-libgdx/spine-libgdx-tests/assets/sack/sack-pma.png"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/celestial-circus/"*
mkdir -p "$ROOT/spine-libgdx/spine-libgdx-tests/assets/celestial-circus/"
cp -f ../celestial-circus/export/* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/celestial-circus/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/snowglobe/"*
mkdir -p "$ROOT/spine-libgdx/spine-libgdx-tests/assets/snowglobe/"
cp -f ../snowglobe/export/* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/snowglobe/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/cloud-pot/"*
mkdir -p "$ROOT/spine-libgdx/spine-libgdx-tests/assets/cloud-pot/"
cp -f ../cloud-pot/export/* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/cloud-pot/"

echo "spine-flutter"
rm -rf "$ROOT/spine-flutter/example/assets/"*
cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-flutter/example/assets/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-flutter/example/assets/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-flutter/example/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-flutter/example/assets/"

cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-flutter/example/assets/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-flutter/example/assets/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-flutter/example/assets/"

cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-flutter/example/assets/"
cp -f ../dragon/export/dragon.atlas "$ROOT/spine-flutter/example/assets/"
cp -f ../dragon/export/dragon.png "$ROOT/spine-flutter/example/assets/"
cp -f ../dragon/export/dragon_*.png "$ROOT/spine-flutter/example/assets/"

cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-flutter/example/assets/"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-flutter/example/assets/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-flutter/example/assets/"

echo "spine-ios"
cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-ios/Example/Spine iOS Example/Assets/celestial/"
cp -f ../celestial-circus/export/celestial-circus-pma.atlas "$ROOT/spine-ios/Example/Spine iOS Example/Assets/celestial/"
cp -f ../celestial-circus/export/celestial-circus-pma.png "$ROOT/spine-ios/Example/Spine iOS Example/Assets/celestial/"

cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-ios/Example/Spine iOS Example/Assets/dragon/"
cp -f ../dragon/export/dragon.atlas "$ROOT/spine-ios/Example/Spine iOS Example/Assets/dragon/"
cp -f ../dragon/export/dragon.png "$ROOT/spine-ios/Example/Spine iOS Example/Assets/dragon/"
cp -f ../dragon/export/dragon_*.png "$ROOT/spine-ios/Example/Spine iOS Example/Assets/dragon/"

cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-ios/Example/Spine iOS Example/Assets/mixandmatch/"
cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$ROOT/spine-ios/Example/Spine iOS Example/Assets/mixandmatch/"
cp -f ../mix-and-match/export/mix-and-match-pma.png "$ROOT/spine-ios/Example/Spine iOS Example/Assets/mixandmatch/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-ios/Example/Spine iOS Example/Assets/spineboy/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ios/Example/Spine iOS Example/Assets/spineboy/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ios/Example/Spine iOS Example/Assets/spineboy/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ios/Example/Spine iOS Example/Assets/spineboy/"

echo "spine-godot"
rm -f "$ROOT"/spine-godot/example/assets/spineboy/*.atlas
rm -f "$ROOT"/spine-godot/example/assets/spineboy/*.png
rm -f "$ROOT"/spine-godot/example/assets/spineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example/assets/spineboy/*.skel
rm -f "$ROOT"/spine-godot/example/assets/raptor/*.atlas
rm -f "$ROOT"/spine-godot/example/assets/raptor/*.png
rm -f "$ROOT"/spine-godot/example/assets/raptor/*.skel
rm -f "$ROOT"/spine-godot/example/assets/mix-and-match/*.atlas
rm -f "$ROOT"/spine-godot/example/assets/mix-and-match/*.png
rm -f "$ROOT"/spine-godot/example/assets/mix-and-match/*.spine-json
rm -f "$ROOT"/spine-godot/example/assets/mix-and-match/*.skel
rm -f "$ROOT"/spine-godot/example/assets/raggedyspineboy/*.atlas
rm -f "$ROOT"/spine-godot/example/assets/raggedyspineboy/*.png
rm -f "$ROOT"/spine-godot/example/assets/raggedyspineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example/assets/celestial-circus/*.atlas
rm -f "$ROOT"/spine-godot/example/assets/celestial-circus/*.png
rm -f "$ROOT"/spine-godot/example/assets/celestial-circus/*.skel


cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-godot/example/assets/spineboy/spineboy-pro.spine-json"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-godot/example/assets/spineboy/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-godot/example/assets/spineboy/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-godot/example/assets/spineboy/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-godot/example/assets/mix-and-match/mix-and-match-pro.spine-json"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-godot/example/assets/mix-and-match/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-godot/example/assets/mix-and-match/"

cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-godot/example/assets/raptor/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-godot/example/assets/raptor/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-godot/example/assets/raptor/"
cp -f ../raptor/manual-maps/raptor-normals.png "$ROOT/spine-godot/example/assets/raptor/n_raptor.png"
cp -f ../raptor/manual-maps/light-sprite.png "$ROOT/spine-godot/example/assets/raptor/light-sprite.png"

cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-godot/example/assets/celestial-circus/celestial-circus.skel"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-godot/example/assets/celestial-circus/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-godot/example/assets/celestial-circus/"

rm -f "$ROOT"/spine-godot/example-v4/assets/spineboy/*.atlas
rm -f "$ROOT"/spine-godot/example-v4/assets/spineboy/*.png
rm -f "$ROOT"/spine-godot/example-v4/assets/spineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4/assets/spineboy/*.skel
rm -f "$ROOT"/spine-godot/example-v4/assets/raptor/*.atlas
rm -f "$ROOT"/spine-godot/example-v4/assets/raptor/*.png
rm -f "$ROOT"/spine-godot/example-v4/assets/raptor/*.skel
rm -f "$ROOT"/spine-godot/example-v4/assets/mix-and-match/*.atlas
rm -f "$ROOT"/spine-godot/example-v4/assets/mix-and-match/*.png
rm -f "$ROOT"/spine-godot/example-v4/assets/mix-and-match/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4/assets/mix-and-match/*.skel
rm -f "$ROOT"/spine-godot/example-v4/assets/raggedyspineboy/*.atlas
rm -f "$ROOT"/spine-godot/example-v4/assets/raggedyspineboy/*.png
rm -f "$ROOT"/spine-godot/example-v4/assets/raggedyspineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4/assets/celestial-circus/*.atlas
rm -f "$ROOT"/spine-godot/example-v4/assets/celestial-circus/*.png
rm -f "$ROOT"/spine-godot/example-v4/assets/celestial-circus/*.skel

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-godot/example-v4/assets/spineboy/spineboy-pro.spine-json"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-godot/example-v4/assets/spineboy/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-godot/example-v4/assets/spineboy/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-godot/example-v4/assets/spineboy/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-godot/example-v4/assets/mix-and-match/mix-and-match-pro.spine-json"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-godot/example-v4/assets/mix-and-match/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-godot/example-v4/assets/mix-and-match/"

cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-godot/example-v4/assets/raptor/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-godot/example-v4/assets/raptor/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-godot/example-v4/assets/raptor/"
cp -f ../raptor/manual-maps/raptor-normals.png "$ROOT/spine-godot/example-v4/assets/raptor/n_raptor.png"
cp -f ../raptor/manual-maps/light-sprite.png "$ROOT/spine-godot/example-v4/assets/raptor/light-sprite.png"

cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-godot/example-v4/assets/celestial-circus/celestial-circus.skel"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-godot/example-v4/assets/celestial-circus/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-godot/example-v4/assets/celestial-circus/"

rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/spineboy/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/spineboy/*.png
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/spineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/spineboy/*.skel
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/raptor/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/raptor/*.png
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/raptor/*.skel
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/mix-and-match/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/mix-and-match/*.png
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/mix-and-match/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/mix-and-match/*.skel
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/raggedyspineboy/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/raggedyspineboy/*.png
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/raggedyspineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/celestial-circus/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/celestial-circus/*.png
rm -f "$ROOT"/spine-godot/example-v4-csharp/assets/celestial-circus/*.skel

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-godot/example-v4-csharp/assets/spineboy/spineboy-pro.spine-json"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-godot/example-v4-csharp/assets/spineboy/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-godot/example-v4-csharp/assets/spineboy/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-godot/example-v4-csharp/assets/spineboy/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-godot/example-v4-csharp/assets/mix-and-match/mix-and-match-pro.spine-json"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-godot/example-v4-csharp/assets/mix-and-match/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-godot/example-v4-csharp/assets/mix-and-match/"

cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-godot/example-v4-csharp/assets/raptor/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-godot/example-v4-csharp/assets/raptor/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-godot/example-v4-csharp/assets/raptor/"
cp -f ../raptor/manual-maps/raptor-normals.png "$ROOT/spine-godot/example-v4-csharp/assets/raptor/n_raptor.png"
cp -f ../raptor/manual-maps/light-sprite.png "$ROOT/spine-godot/example-v4-csharp/assets/raptor/light-sprite.png"

cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-godot/example-v4-csharp/assets/celestial-circus/celestial-circus.skel"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-godot/example-v4-csharp/assets/celestial-circus/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-godot/example-v4-csharp/assets/celestial-circus/"

rm -f "$ROOT"/spine-godot/example-v4-extension/assets/spineboy/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/spineboy/*.png
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/spineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/spineboy/*.skel
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/raptor/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/raptor/*.png
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/raptor/*.skel
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/mix-and-match/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/mix-and-match/*.png
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/mix-and-match/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/mix-and-match/*.skel
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/raggedyspineboy/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/raggedyspineboy/*.png
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/raggedyspineboy/*.spine-json
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/celestial-circus/*.atlas
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/celestial-circus/*.png
rm -f "$ROOT"/spine-godot/example-v4-extension/assets/celestial-circus/*.skel

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-godot/example-v4-extension/assets/spineboy/spineboy-pro.spine-json"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-godot/example-v4-extension/assets/spineboy/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-godot/example-v4-extension/assets/spineboy/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-godot/example-v4-extension/assets/spineboy/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-godot/example-v4-extension/assets/mix-and-match/mix-and-match-pro.spine-json"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-godot/example-v4-extension/assets/mix-and-match/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-godot/example-v4-extension/assets/mix-and-match/"

cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-godot/example-v4-extension/assets/raptor/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-godot/example-v4-extension/assets/raptor/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-godot/example-v4-extension/assets/raptor/"
cp -f ../raptor/manual-maps/raptor-normals.png "$ROOT/spine-godot/example-v4-extension/assets/raptor/n_raptor.png"
cp -f ../raptor/manual-maps/light-sprite.png "$ROOT/spine-godot/example-v4-extension/assets/raptor/light-sprite.png"

cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-godot/example-v4-extension/assets/celestial-circus/celestial-circus.skel"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-godot/example-v4-extension/assets/celestial-circus/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-godot/example-v4-extension/assets/celestial-circus/"

echo "spine-sdl"
rm -f "$ROOT/spine-sdl/data/"*
cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-sdl/data/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-sdl/data/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-sdl/data/"

echo "spine-glfw"
rm -f "$ROOT/spine-glfw/data/"*
cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-glfw/data/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-glfw/data/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-glfw/data/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-glfw/data/"
cp -f ../celestial-circus/export/celestial-circus-pro.json "$ROOT/spine-glfw/data/"
cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-glfw/data/"
cp -f ../celestial-circus/export/celestial-circus-pma.atlas "$ROOT/spine-glfw/data/"
cp -f ../celestial-circus/export/celestial-circus-pma.png "$ROOT/spine-glfw/data/"
cp -f ../dragon/export/dragon-ess.json "$ROOT/spine-glfw/data/"
cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-glfw/data/"
cp -f ../dragon/export/dragon-pma.atlas "$ROOT/spine-glfw/data/"
cp -f ../dragon/export/dragon-pma*.png "$ROOT/spine-glfw/data/"

echo "spine-sfml"
rm -f "$ROOT/spine-sfml/data/"*
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-sfml/data/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-sfml/data/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-sfml/data/"

echo "spine-ts"
rm -f "$ROOT/spine-ts/assets/"*

cp -f ../celestial-circus/export/celestial-circus-pro.json "$ROOT/spine-ts/assets/"
cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../celestial-circus/export/celestial-circus-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../celestial-circus/export/celestial-circus-pma.png "$ROOT/spine-ts/assets/"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-ts/assets/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-ts/assets/"

cp -f ../chibi-stickers/export/chibi-stickers.json "$ROOT/spine-ts/assets/"
cp -f ../chibi-stickers/export/chibi-stickers.skel "$ROOT/spine-ts/assets/"
cp -f ../chibi-stickers/export/chibi-stickers.atlas "$ROOT/spine-ts/assets/"
cp -f ../chibi-stickers/export/chibi-stickers-pma* "$ROOT/spine-ts/assets/"

cp -f ../cloud-pot/export/cloud-pot.skel "$ROOT/spine-ts/assets/"
cp -f ../cloud-pot/export/cloud-pot-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../cloud-pot/export/cloud-pot-pma.png "$ROOT/spine-ts/assets/"
cp -f ../cloud-pot/export/cloud-pot.atlas "$ROOT/spine-ts/assets/"
cp -f ../cloud-pot/export/cloud-pot.png "$ROOT/spine-ts/assets/"

cp -f ../coin/export/coin-pro.json "$ROOT/spine-ts/assets/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../coin/export/coin-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../coin/export/coin-pma.png "$ROOT/spine-ts/assets/"

cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-ts/assets/"
cp -f ../dragon/export/dragon-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../dragon/export/dragon-pma*.png "$ROOT/spine-ts/assets/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-ts/assets/"
cp -f ../goblins/export/goblins-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../goblins/export/goblins-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../goblins/export/goblins-pma.png "$ROOT/spine-ts/assets/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-ts/assets/"
cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../mix-and-match/export/mix-and-match-pma.png "$ROOT/spine-ts/assets/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-ts/assets/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-ts/assets/"

cp -f ../owl/export/owl-pro.json "$ROOT/spine-ts/assets/"
cp -f ../owl/export/owl-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../owl/export/owl-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../owl/export/owl-pma.png "$ROOT/spine-ts/assets/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ts/assets/"
cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../raptor/export/raptor-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../raptor/export/raptor-pma.png "$ROOT/spine-ts/assets/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-ts/assets/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-ts/assets/"
cp -f ../raptor/images/raptor-jaw-tooth.png "$ROOT/spine-ts/assets/"

cp -f ../7-anticipation/export/sack-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../7-anticipation/export/7-anticipation-pma.atlas "$ROOT/spine-ts/assets/sack-pma.atlas"
$sed -i 's/7-anticipation-pma.png/sack-pma.png/g' "$ROOT/spine-ts/assets/sack-pma.atlas"
cp -f ../7-anticipation/export/7-anticipation-pma.png "$ROOT/spine-ts/assets/sack-pma.png"
cp -f ../7-anticipation/export/7-anticipation.atlas "$ROOT/spine-ts/assets/sack.atlas"
$sed -i 's/7-anticipation.png/sack.png/g' "$ROOT/spine-ts/assets/sack.atlas"
cp -f ../7-anticipation/export/7-anticipation.png "$ROOT/spine-ts/assets/sack.png"

cp -f ../snowglobe/export/snowglobe-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../snowglobe/export/snowglobe-pma* "$ROOT/spine-ts/assets/"
cp -f ../snowglobe/export/snowglobe* "$ROOT/spine-ts/assets/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-ts/assets/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../spineboy/export/spineboy-ess.json "$ROOT/spine-ts/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/assets/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-ts/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-ts/assets/"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-ts/assets/"
cp -f ../stretchyman/export/stretchyman-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../stretchyman/export/stretchyman-pma.png "$ROOT/spine-ts/assets/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-ts/assets/"
cp -f ../tank/export/tank-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../tank/export/tank-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../tank/export/tank-pma.png "$ROOT/spine-ts/assets/"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-ts/assets/"
cp -f ../vine/export/vine-pro.skel "$ROOT/spine-ts/assets/"
cp -f ../vine/export/vine-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../vine/export/vine-pma.png "$ROOT/spine-ts/assets/"

cp -f ../windmill/export/windmill-ess.json "$ROOT/spine-ts/assets/"
cp -f ../windmill/export/windmill-pma.atlas "$ROOT/spine-ts/assets/"
cp -f ../windmill/export/windmill-pma.png "$ROOT/spine-ts/assets/"

cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/spine-phaser-v3/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/spine-phaser-v3/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/spine-phaser-v3/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/spine-phaser-v4/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/spine-phaser-v4/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/spine-phaser-v4/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/spine-pixi-v7/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/spine-pixi-v7/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/spine-pixi-v7/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/spine-pixi-v8/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/spine-pixi-v8/example/typescript/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/spine-pixi-v8/example/typescript/assets/"


echo "spine-monogame"
rm -f "$ROOT/spine-monogame/spine-monogame-example/data/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../coin/export/coin.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../coin/export/coin.png "$ROOT/spine-monogame/spine-monogame-example/data/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-monogame/spine-monogame-example/data/"
# Note: normalmap need to be created manually. Thus we use a separately prepared atlas and
# diffuse map so that the maps always match. These atlas textures are copied to the target dir.
cp -f ../raptor/manual-maps/raptor.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../raptor/manual-maps/raptor.png "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../raptor/manual-maps/raptor-normals.png "$ROOT/spine-monogame/spine-monogame-example/data/raptor_normals.png"

cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-monogame/spine-monogame-example/data/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../tank/export/tank.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../tank/export/tank.png "$ROOT/spine-monogame/spine-monogame-example/data/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-monogame/spine-monogame-example/data/"

cp -f ../celestial-circus/export/celestial-circus-pro.json "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-monogame/spine-monogame-example/data/"

cp -f ../snowglobe/export/snowglobe-pro.skel "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../snowglobe/export/snowglobe.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../snowglobe/export/snowglobe.png "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../snowglobe/export/snowglobe_*.png "$ROOT/spine-monogame/spine-monogame-example/data/"

cp -f ../cloud-pot/export/cloud-pot.skel "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../cloud-pot/export/cloud-pot.atlas "$ROOT/spine-monogame/spine-monogame-example/data/"
cp -f ../cloud-pot/export/cloud-pot.png "$ROOT/spine-monogame/spine-monogame-example/data/"

echo "spine-haxe"
rm -f "$ROOT/spine-haxe/example/assets/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../coin/export/coin.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../coin/export/coin.png "$ROOT/spine-haxe/example/assets/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../goblins/export/goblins-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../goblins/export/goblins.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../goblins/export/goblins.png "$ROOT/spine-haxe/example/assets/"

cp -f ../dragon/export/dragon-ess.json "$ROOT/spine-haxe/example/assets/"
cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../dragon/export/dragon.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../dragon/export/dragon*.png "$ROOT/spine-haxe/example/assets/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-haxe/example/assets/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-haxe/example/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-haxe/example/assets/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../tank/export/tank-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../tank/export/tank.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../tank/export/tank.png "$ROOT/spine-haxe/example/assets/"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../vine/export/vine-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../vine/export/vine.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../vine/export/vine.png "$ROOT/spine-haxe/example/assets/"

cp -f ../owl/export/owl-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../owl/export/owl-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../owl/export/owl.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../owl/export/owl.png "$ROOT/spine-haxe/example/assets/"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../stretchyman/export/stretchyman-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../stretchyman/export/stretchyman.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../stretchyman/export/stretchyman.png "$ROOT/spine-haxe/example/assets/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-haxe/example/assets/"

cp -f ../celestial-circus/export/* "$ROOT/spine-haxe/example/assets/"

cp -f ../cloud-pot/export/cloud-pot.json "$ROOT/spine-haxe/example/assets/"
cp -f ../cloud-pot/export/cloud-pot.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../cloud-pot/export/cloud-pot.atlas "$ROOT/spine-haxe/example/assets/"
cp -f ../cloud-pot/export/cloud-pot.png "$ROOT/spine-haxe/example/assets/"

cp -f ../7-anticipation/export/sack-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../7-anticipation/export/sack-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../7-anticipation/export/7-anticipation.atlas "$ROOT/spine-haxe/example/assets/sack.atlas"
$sed -i 's/7-anticipation.png/sack.png/g' "$ROOT/spine-haxe/example/assets/sack.atlas"
cp -f ../7-anticipation/export/7-anticipation.png "$ROOT/spine-haxe/example/assets/sack.png"

cp -f ../snowglobe/export/snowglobe-pro.json "$ROOT/spine-haxe/example/assets/"
cp -f ../snowglobe/export/snowglobe-pro.skel "$ROOT/spine-haxe/example/assets/"
cp -f ../snowglobe/export/snowglobe* "$ROOT/spine-haxe/example/assets/"

echo "spine-ue"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Raptor/raptor.json"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Raptor/raptor-pro.json"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Raptor/raptor.atlas"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Raptor/raptor.png"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Spineboy/spineboy.json"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Spineboy/spineboy-pro.json"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Spineboy/spineboy.atlas"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/Spineboy/spineboy.png"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match-pro.skel"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match-pro-skeleton.skel"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match-pro.atlas"
rm -f "$ROOT/spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match-pro.png"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ue/Content/GettingStarted/Assets/Raptor/raptor-pro.json"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-ue/Content/GettingStarted/Assets/Raptor/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-ue/Content/GettingStarted/Assets/Raptor/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-ue/Content/GettingStarted/Assets/Spineboy/spineboy-pro.json"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-ue/Content/GettingStarted/Assets/Spineboy/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-ue/Content/GettingStarted/Assets/Spineboy/"

cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match.png"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match.atlas"
cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match-pro.skel"

cp -f ../celestial-circus/export/celestial-circus-pro.skel "$ROOT/spine-ue/Content/GettingStarted/Assets/celestial-circus/"
cp -f ../celestial-circus/export/celestial-circus.atlas "$ROOT/spine-ue/Content/GettingStarted/Assets/celestial-circus/"
cp -f ../celestial-circus/export/celestial-circus.png "$ROOT/spine-ue/Content/GettingStarted/Assets/celestial-circus/"

echo "spine-unity"

# Section of assets specific for the spine-unity runtime.
UNITY_SOURCE_DIR=../spine-unity

# Do not delete everything in unity dirs, especially not .meta files.
# Note: We copy the files following the existing naming scheme (e.g. goblins.json instead of goblins-pro.json)
#       to the unity assets directories. This requires to change the png file reference line in the atlas file.
UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Dragon"
cp -f ../dragon/export/dragon-ess.json "$UNITY_TARGET_DIR/dragon.json"
cp -f ../dragon/export/dragon.atlas "$UNITY_TARGET_DIR/dragon.atlas.txt"
cp -f ../dragon/export/dragon.png "$UNITY_TARGET_DIR/"
cp -f ../dragon/export/dragon_*.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Goblins"
cp -f ../goblins/export/goblins-pro.json "$UNITY_TARGET_DIR/goblins.json"
cp -f ../goblins/export/goblins.atlas "$UNITY_TARGET_DIR/goblins.atlas.txt"
cp -f ../goblins/export/goblins.png "$UNITY_TARGET_DIR/goblins.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Hero"
cp -f ../hero/export/hero-pro.json "$UNITY_TARGET_DIR/"
cp -f ../hero/export/hero.atlas "$UNITY_TARGET_DIR/hero-pro.atlas.txt"
$sed -i 's/hero.png/hero-pro.png/g' "$UNITY_TARGET_DIR/hero-pro.atlas.txt"
cp -f ../hero/export/hero.png "$UNITY_TARGET_DIR/hero-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/raptor-pro-and-mask"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/raptor-pro.json"
cp -f ../raptor/export/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/export/raptor.png "$UNITY_TARGET_DIR/raptor.png"

# URP packages
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Samples~/Examples/2D/Spine Skeletons/RaptorURP"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/"
cp -f ../raptor/manual-maps/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/manual-maps/raptor.png "$UNITY_TARGET_DIR/"
cp -f ../raptor/manual-maps/raptor-rim-mask.png "$UNITY_TARGET_DIR/"
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Samples~/Examples/3D/Spine Skeletons/RaptorURP"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/"
cp -f ../raptor/export/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/export/raptor.png "$UNITY_TARGET_DIR/"
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.ui-toolkit/Samples~/Examples/Spine Skeletons/Raptor"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/"
cp -f ../raptor/export/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/export/raptor.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/spineboy-pro"
cp -f ../spineboy/export/spineboy-pro.json "$UNITY_TARGET_DIR/spineboy-pro.json"
cp -f ../spineboy/export/spineboy.atlas "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
$sed -i 's/spineboy.png/spineboy-pro.png/g' "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
cp -f ../spineboy/export/spineboy.png "$UNITY_TARGET_DIR/spineboy-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/mix-and-match"
cp -f ../mix-and-match/export/mix-and-match-pro.json "$UNITY_TARGET_DIR/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$UNITY_TARGET_DIR/mix-and-match.atlas.txt"
cp -f ../mix-and-match/export/mix-and-match.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Stretchyman"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
# Note: normalmap and emissionmap need to be created manually. Thus we use a separately prepared
# atlas and diffuse map here so that the maps always match. These atlas textures are copied to the target dir.
cp -f ../stretchyman/manual-maps/stretchyman.atlas "$UNITY_TARGET_DIR/stretchyman-diffuse.atlas.txt"
$sed -i 's/stretchyman.png/stretchyman-diffuse.png/g' "$UNITY_TARGET_DIR/stretchyman-diffuse.atlas.txt"
cp -f ../stretchyman/manual-maps/stretchyman.png "$UNITY_TARGET_DIR/stretchyman-diffuse.png"
cp -f ../stretchyman/manual-maps/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-emission.png "$UNITY_TARGET_DIR/"

# URP packages
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Samples~/Examples/2D/Spine Skeletons/StretchymanURP"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
cp -f ../stretchyman/manual-maps/stretchyman.atlas "$UNITY_TARGET_DIR/stretchyman.atlas.txt"
cp -f ../stretchyman/manual-maps/stretchyman.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-emission.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-rim-mask.png "$UNITY_TARGET_DIR/"
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Samples~/Examples/3D/Spine Skeletons/StretchymanURP"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
cp -f ../stretchyman/manual-maps/stretchyman.atlas "$UNITY_TARGET_DIR/stretchyman.atlas.txt"
cp -f ../stretchyman/manual-maps/stretchyman.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-emission.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Eyes"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes.json "$UNITY_TARGET_DIR/eyes.json"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes.atlas "$UNITY_TARGET_DIR/eyes.atlas.txt"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes.png "$UNITY_TARGET_DIR/eyes.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/FootSoldier"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier.json "$UNITY_TARGET_DIR/FootSoldier.json"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier.atlas "$UNITY_TARGET_DIR/FS_White.atlas.txt"
$sed -i 's/footsoldier.png/FS_White.png/g' "$UNITY_TARGET_DIR/FS_White.atlas.txt"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier.png "$UNITY_TARGET_DIR/FS_White.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Gauge"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge.json "$UNITY_TARGET_DIR/Gauge.json"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge.atlas "$UNITY_TARGET_DIR/Gauge.atlas.txt"
$sed -i 's/gauge.png/Gauge.png/g' "$UNITY_TARGET_DIR/Gauge.atlas.txt"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge.png "$UNITY_TARGET_DIR/Gauge.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Raptor"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor.json "$UNITY_TARGET_DIR/raptor.json"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor.png "$UNITY_TARGET_DIR/raptor.png"


UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Raggedy Spineboy"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy.json "$UNITY_TARGET_DIR/raggedy spineboy.json"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy.atlas "$UNITY_TARGET_DIR/Raggedy Spineboy.atlas.txt"
$sed -i 's/raggedyspineboy.png/Raggedy Spineboy.png/g' "$UNITY_TARGET_DIR/Raggedy Spineboy.atlas.txt"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy.png "$UNITY_TARGET_DIR/Raggedy Spineboy.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/spineboy-pro"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy-pro.json "$UNITY_TARGET_DIR/spineboy-pro.json"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy.atlas "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
$sed -i 's/spineboy.png/spineboy-pro.png/g' "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy.png "$UNITY_TARGET_DIR/spineboy-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/spineboy-unity"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy-unity.json "$UNITY_TARGET_DIR/spineboy-unity.json"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy.atlas "$UNITY_TARGET_DIR/spineboy.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy.png "$UNITY_TARGET_DIR/spineboy.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Spineunitygirl"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi.json "$UNITY_TARGET_DIR/Doi.json"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi.atlas "$UNITY_TARGET_DIR/Doi.atlas.txt"
$sed -i 's/doi.png/Doi.png/g' "$UNITY_TARGET_DIR/Doi.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi.png "$UNITY_TARGET_DIR/Doi.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/Whirlyblendmodes"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes.json "$UNITY_TARGET_DIR/whirlyblendmodes.json"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes.atlas "$UNITY_TARGET_DIR/whirlyblendmodes.atlas.txt"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes.png "$UNITY_TARGET_DIR/whirlyblendmodes.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/celestial-circus"
cp -f ../celestial-circus/export/celestial-circus-pro.json "$UNITY_TARGET_DIR/"
cp -f ../celestial-circus/export/celestial-circus.atlas "$UNITY_TARGET_DIR/celestial-circus.atlas.txt"
cp -f ../celestial-circus/export/celestial-circus.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/snowglobe"
cp -f ../snowglobe/export/snowglobe-pro.skel "$UNITY_TARGET_DIR/snowglobe-pro.skel.bytes"
cp -f ../snowglobe/export/snowglobe.atlas "$UNITY_TARGET_DIR/snowglobe.atlas.txt"
cp -f ../snowglobe/export/snowglobe.png "$UNITY_TARGET_DIR/"
cp -f ../snowglobe/export/snowglobe_2.png "$UNITY_TARGET_DIR/"
cp -f ../snowglobe/export/snowglobe_3.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/cloud-pot"
cp -f ../cloud-pot/export/cloud-pot.skel "$UNITY_TARGET_DIR/cloud-pot.skel.bytes"
cp -f ../cloud-pot/export/cloud-pot.atlas "$UNITY_TARGET_DIR/cloud-pot.atlas.txt"
cp -f ../cloud-pot/export/cloud-pot.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine/Samples~/Spine Examples/Spine Skeletons/sack"
cp -f ../7-anticipation/export/sack-pro.skel "$UNITY_TARGET_DIR/sack-pro.skel.bytes"
cp -f ../7-anticipation/export/7-anticipation.atlas "$UNITY_TARGET_DIR/sack.atlas.txt"
$sed -i 's/7-anticipation.png/sack.png/g' "$UNITY_TARGET_DIR/sack.atlas.txt"
cp -f ../7-anticipation/export/7-anticipation.png "$UNITY_TARGET_DIR/sack.png"

echo "--"
echo "Note regarding spine-xna and spine-unity:"
echo "Some textures (normalmap, emission, rim-mask) need manual update."
echo "Please update the following maps when an atlas update is needed:"
echo "[xna and unity]     'raptor/manual-maps' to match png in 'raptor/export/'"
echo "[unity only]   'stretchyman/manual-maps' to match png in 'stretchyman/export/'"
echo "If not updated, the old consistent file-set in the directory is used."
echo "--"
