#!/bin/sh
set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
cd $SCRIPT_DIR

ROOT=$SCRIPT_DIR/../..
echo "Spine Runtimes path: $ROOT"
echo "Copying assets to runtimes..."
echo ""

echo "spine-libgdx"
rm "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"*
cp -f ../goblins/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"
cp -f ../goblins/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"
cp -f ../goblins/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/goblins/"

rm "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"*
cp -f ../raptor/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"
cp -f ../raptor/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"
cp -f ../raptor/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/raptor/"

rm "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"*
cp -f ../spineboy/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"
cp -r ../spineboy/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"
cp -r ../spineboy/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/spineboy/"

rm "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"*
cp -f ../coin/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"
cp -f ../coin/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"
cp -f ../coin/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/coin/"

rm -f "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"*
cp -f ../mix-and-match/export/*.json "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"
cp -f ../mix-and-match/export/*.skel "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"
cp -f ../mix-and-match/export/*-pma.* "$ROOT/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"

echo "spine-cocos2d-objc"
rm "$ROOT/spine-cocos2d-objc/Resources/"*

cp -f ../coin/export/coin-pro.json "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../coin/export/coin.atlas "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../coin/export/coin.png "$ROOT/spine-cocos2d-objc/Resources/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../goblins/export/goblins.atlas "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../goblins/export/goblins.png "$ROOT/spine-cocos2d-objc/Resources/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-cocos2d-objc/Resources/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-cocos2d-objc/Resources/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../tank/export/tank.atlas "$ROOT/spine-cocos2d-objc/Resources/"
cp -f ../tank/export/tank.png "$ROOT/spine-cocos2d-objc/Resources/"

echo "spine-cocos2dx"
rm -rf "$ROOT/spine-cocos2dx/example/Resources/common/"*

cp -f ../coin/export/coin-pro.skel "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../coin/export/coin.atlas "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../coin/export/coin.png "$ROOT/spine-cocos2dx/example/Resources/common/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../goblins/export/goblins.atlas "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../goblins/export/goblins.png "$ROOT/spine-cocos2dx/example/Resources/common/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-cocos2dx/example/Resources/common/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-cocos2dx/example/Resources/common/"

cp -f ../tank/export/tank-pro.skel "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../tank/export/tank.atlas "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../tank/export/tank.png "$ROOT/spine-cocos2dx/example/Resources/common/"

cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-cocos2dx/example/Resources/common/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-cocos2dx/example/Resources/common/"

rm "$ROOT/spine-cocos2dx/example-v4/Resources/common/"*

cp -f ../coin/export/coin-pro.skel "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../coin/export/coin.atlas "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../coin/export/coin.png "$ROOT/spine-cocos2dx/example-v4/Resources/common/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../goblins/export/goblins.atlas "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../goblins/export/goblins.png "$ROOT/spine-cocos2dx/example-v4/Resources/common/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-cocos2dx/example-v4/Resources/common/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-cocos2dx/example-v4/Resources/common/"

cp -f ../tank/export/tank-pro.skel "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../tank/export/tank.atlas "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../tank/export/tank.png "$ROOT/spine-cocos2dx/example-v4/Resources/common/"

cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-cocos2dx/example-v4/Resources/common/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-cocos2dx/example-v4/Resources/common/"

echo "spine-sfml-c"
rm "$ROOT/spine-sfml/c/data/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../coin/export/coin-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../coin/export/coin-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../dragon/export/dragon-ess.json "$ROOT/spine-sfml/c/data/"
cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-sfml/c/data/"
cp -f ../dragon/export/dragon-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../dragon/export/dragon-pma*.png "$ROOT/spine-sfml/c/data/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../goblins/export/goblins-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../goblins/export/goblins-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../goblins/export/goblins-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../raptor/export/raptor-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../raptor/export/raptor-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../tank/export/tank-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../tank/export/tank-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../tank/export/tank-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../vine/export/vine-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../vine/export/vine-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../vine/export/vine-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../stretchyman/export/stretchyman-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../stretchyman/export/stretchyman-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../owl/export/owl-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../owl/export/owl-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../owl/export/owl-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../owl/export/owl-pma.png "$ROOT/spine-sfml/c/data/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../mix-and-match/export/mix-and-match-pma.png "$ROOT/spine-sfml/c/data/"

echo "spine-sfml-cpp"
rm "$ROOT/spine-sfml/cpp/data/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../coin/export/coin-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../coin/export/coin-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../dragon/export/dragon-ess.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../dragon/export/dragon-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../dragon/export/dragon-pma*.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../goblins/export/goblins-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../goblins/export/goblins-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../goblins/export/goblins-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../raptor/export/raptor-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../raptor/export/raptor-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../tank/export/tank-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../tank/export/tank-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../tank/export/tank-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../vine/export/vine-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../vine/export/vine-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../vine/export/vine-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../stretchyman/export/stretchyman-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../stretchyman/export/stretchyman-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../owl/export/owl-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../owl/export/owl-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../owl/export/owl-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../owl/export/owl-pma.png "$ROOT/spine-sfml/cpp/data/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-sfml/cpp/data/"
cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-sfml/cpp/data/"
cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$ROOT/spine-sfml/cpp/data/"
cp -f ../mix-and-match/export/mix-and-match-pma.png "$ROOT/spine-sfml/cpp/data/"

echo "spine-ts"
rm "$ROOT/spine-ts/spine-webgl/example/assets/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../coin/export/coin-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../coin/export/coin-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../goblins/export/goblins-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../goblins/export/goblins-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../goblins/export/goblins-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../dragon/export/dragon-ess.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../dragon/export/dragon-ess.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../dragon/export/dragon-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../dragon/export/dragon-pma*.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../raptor/export/raptor-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../raptor/export/raptor-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../tank/export/tank-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../tank/export/tank-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../tank/export/tank-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../vine/export/vine-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../vine/export/vine-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../vine/export/vine-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../owl/export/owl-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../owl/export/owl-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../owl/export/owl-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../owl/export/owl-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../stretchyman/export/stretchyman-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../stretchyman/export/stretchyman-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$ROOT/spine-ts/spine-webgl/example/assets/"
cp -f ../mix-and-match/export/mix-and-match-pma.png "$ROOT/spine-ts/spine-webgl/example/assets/"

rm "$ROOT/spine-ts/spine-canvas/example/assets/"*
cp -f ../spineboy/export/spineboy-ess.json "$ROOT/spine-ts/spine-canvas/example/assets/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-ts/spine-canvas/example/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-ts/spine-canvas/example/assets/"

rm "$ROOT/spine-ts/spine-threejs/example/assets/"*
cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ts/spine-threejs/example/assets/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-ts/spine-threejs/example/assets/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-ts/spine-threejs/example/assets/"

rm "$ROOT/spine-ts/spine-player/example/assets/"*
cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ts/spine-player/example/assets/"
cp -f ../raptor/export/raptor-pma.atlas "$ROOT/spine-ts/spine-player/example/assets/"
cp -f ../raptor/export/raptor-pma.png "$ROOT/spine-ts/spine-player/example/assets/"

cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/spine-player/example/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/spine-player/example/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/spine-player/example/assets/"

echo "spine-monogame"
rm "$ROOT/spine-monogame/spine-monogame-example/data/"*
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

echo "spine-ue4"
rm "$ROOT/spine-ue4/Content/GettingStarted/Assets/Raptor/raptor.json"
rm "$ROOT/spine-ue4/Content/GettingStarted/Assets/Raptor/raptor.atlas"
rm "$ROOT/spine-ue4/Content/GettingStarted/Assets/Raptor/raptor.png"
rm "$ROOT/spine-ue4/Content/GettingStarted/Assets/Spineboy/spineboy.json"
rm "$ROOT/spine-ue4/Content/GettingStarted/Assets/Spineboy/spineboy.atlas"
rm "$ROOT/spine-ue4/Content/GettingStarted/Assets/Spineboy/spineboy.png"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ue4/Content/GettingStarted/Assets/Raptor/raptor.json"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-ue4/Content/GettingStarted/Assets/Raptor/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-ue4/Content/GettingStarted/Assets/Raptor/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-ue4/Content/GettingStarted/Assets/Spineboy/spineboy.json"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-ue4/Content/GettingStarted/Assets/Spineboy/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-ue4/Content/GettingStarted/Assets/Spineboy/"

echo "spine-unity"

# Section of assets specific for the spine-unity runtime.
UNITY_SOURCE_DIR=../spine-unity

# On macOS, we need gsed for the -i switch to work. Check it's available
# and error out otherwise.
sed="sed"
if [[ $OSTYPE == 'darwin'* ]]; then
  if [ ! -f "/usr/local/bin/gsed" ]; then
	echo "macOS sed detected. Please install GNU sed via brew install gnu-sed"
	exit -1
  fi
  sed="/usr/local/bin/gsed"
fi

# Do not delete everything in unity dirs, especially not .meta files.
# Note: We copy the files following the existing naming scheme (e.g. goblins.json instead of goblins-pro.json)
#       to the unity assets directories. This requires to change the png file reference line in the atlas file.
UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Dragon"
cp -f ../dragon/export/dragon-ess.json "$UNITY_TARGET_DIR/dragon.json"
cp -f ../dragon/export/dragon-pma.atlas "$UNITY_TARGET_DIR/dragon.atlas.txt"
$sed -i "s/dragon-pma.png/dragon.png/g" "$UNITY_TARGET_DIR/dragon.atlas.txt"
$sed -i "s/dragon-pma_2.png/dragon2.png/g" "$UNITY_TARGET_DIR/dragon.atlas.txt"
$sed -i "s/dragon-pma_3.png/dragon3.png/g" "$UNITY_TARGET_DIR/dragon.atlas.txt"
$sed -i "s/dragon-pma_4.png/dragon4.png/g" "$UNITY_TARGET_DIR/dragon.atlas.txt"
$sed -i "s/dragon-pma_5.png/dragon5.png/g" "$UNITY_TARGET_DIR/dragon.atlas.txt"
$sed -i "s/dragon-pma_6.png/dragon6.png/g" "$UNITY_TARGET_DIR/dragon.atlas.txt"
cp -f ../dragon/export/dragon-pma.png "$UNITY_TARGET_DIR/dragon.png"
cp -f ../dragon/export/dragon-pma_2.png "$UNITY_TARGET_DIR/dragon2.png"
cp -f ../dragon/export/dragon-pma_3.png "$UNITY_TARGET_DIR/dragon3.png"
cp -f ../dragon/export/dragon-pma_4.png "$UNITY_TARGET_DIR/dragon4.png"
cp -f ../dragon/export/dragon-pma_5.png "$UNITY_TARGET_DIR/dragon5.png"
cp -f ../dragon/export/dragon-pma_6.png "$UNITY_TARGET_DIR/dragon6.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Goblins"
cp -f ../goblins/export/goblins-pro.json "$UNITY_TARGET_DIR/goblins.json"
cp -f ../goblins/export/goblins-pma.atlas "$UNITY_TARGET_DIR/goblins.atlas.txt"
$sed -i 's/goblins-pma.png/goblins.png/g' "$UNITY_TARGET_DIR/goblins.atlas.txt"
cp -f ../goblins/export/goblins-pma.png "$UNITY_TARGET_DIR/goblins.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Hero"
cp -f ../hero/export/hero-pro.json "$UNITY_TARGET_DIR/"
cp -f ../hero/export/hero-pma.atlas "$UNITY_TARGET_DIR/hero-pro.atlas.txt"
$sed -i 's/hero-pma.png/hero-pro.png/g' "$UNITY_TARGET_DIR/hero-pro.atlas.txt"
cp -f ../hero/export/hero-pma.png "$UNITY_TARGET_DIR/hero-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/raptor-pro-and-mask"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/raptor-pro.json"
cp -f ../raptor/export/raptor-pma.atlas "$UNITY_TARGET_DIR/raptor-pma.atlas.txt"
cp -f ../raptor/export/raptor-pma.png "$UNITY_TARGET_DIR/raptor-pma.png"

# URP packages
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Examples/2D/Spine Skeletons/RaptorURP"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/"
cp -f ../raptor/manual-maps/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/manual-maps/raptor.png "$UNITY_TARGET_DIR/"
cp -f ../raptor/manual-maps/raptor-rim-mask.png "$UNITY_TARGET_DIR/"
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Examples/3D/Spine Skeletons/RaptorURP"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/"
cp -f ../raptor/export/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/export/raptor.png "$UNITY_TARGET_DIR/"
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.lwrp-shaders/Spine Skeletons/RaptorLWRP"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/"
cp -f ../raptor/export/raptor.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/export/raptor.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-pro"
cp -f ../spineboy/export/spineboy-pro.json "$UNITY_TARGET_DIR/spineboy-pro.json"
cp -f ../spineboy/export/spineboy-pma.atlas "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
$sed -i 's/spineboy-pma.png/spineboy-pro.png/g' "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
cp -f ../spineboy/export/spineboy-pma.png "$UNITY_TARGET_DIR/spineboy-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/mix-and-match"
cp -f ../mix-and-match/export/mix-and-match-pro.json "$UNITY_TARGET_DIR/"
cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$UNITY_TARGET_DIR/mix-and-match-pma.atlas.txt"
cp -f ../mix-and-match/export/mix-and-match-pma.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Stretchyman"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
# Note: normalmap and emissionmap need to be created manually. Thus we use a separately prepared
# atlas and diffuse map here so that the maps always match. These atlas textures are copied to the target dir.
cp -f ../stretchyman/manual-maps/stretchyman-pma.atlas "$UNITY_TARGET_DIR/stretchyman-diffuse-pma.atlas.txt"
$sed -i 's/stretchyman-pma.png/stretchyman-diffuse-pma.png/g' "$UNITY_TARGET_DIR/stretchyman-diffuse-pma.atlas.txt"
cp -f ../stretchyman/manual-maps/stretchyman-pma.png "$UNITY_TARGET_DIR/stretchyman-diffuse-pma.png"
cp -f ../stretchyman/manual-maps/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-emission.png "$UNITY_TARGET_DIR/"

# URP packages
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Examples/2D/Spine Skeletons/StretchymanURP"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
cp -f ../stretchyman/manual-maps/stretchyman.atlas "$UNITY_TARGET_DIR/stretchyman.atlas.txt"
cp -f ../stretchyman/manual-maps/stretchyman.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-emission.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-rim-mask.png "$UNITY_TARGET_DIR/"
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.urp-shaders/Examples/3D/Spine Skeletons/StretchymanURP"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
cp -f ../stretchyman/manual-maps/stretchyman.atlas "$UNITY_TARGET_DIR/stretchyman.atlas.txt"
cp -f ../stretchyman/manual-maps/stretchyman.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-emission.png "$UNITY_TARGET_DIR/"
UNITY_TARGET_DIR="$ROOT/spine-unity/Modules/com.esotericsoftware.spine.lwrp-shaders/Spine Skeletons/StretchymanLWRP"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
cp -f ../stretchyman/manual-maps/stretchyman.atlas "$UNITY_TARGET_DIR/stretchyman.atlas.txt"
cp -f ../stretchyman/manual-maps/stretchyman.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f ../stretchyman/manual-maps/stretchyman-emission.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Eyes"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes.json "$UNITY_TARGET_DIR/eyes.json"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes-pma.atlas "$UNITY_TARGET_DIR/eyes.atlas.txt"
$sed -i 's/eyes-pma.png/eyes.png/g' "$UNITY_TARGET_DIR/eyes.atlas.txt"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes-pma.png "$UNITY_TARGET_DIR/eyes.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/FootSoldier"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier.json "$UNITY_TARGET_DIR/FootSoldier.json"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier-pma.atlas "$UNITY_TARGET_DIR/FS_White.atlas.txt"
$sed -i 's/footsoldier-pma.png/FS_White.png/g' "$UNITY_TARGET_DIR/FS_White.atlas.txt"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier-pma.png "$UNITY_TARGET_DIR/FS_White.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Gauge"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge.json "$UNITY_TARGET_DIR/Gauge.json"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge-pma.atlas "$UNITY_TARGET_DIR/Gauge.atlas.txt"
$sed -i 's/gauge-pma.png/Gauge.png/g' "$UNITY_TARGET_DIR/Gauge.atlas.txt"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge-pma.png "$UNITY_TARGET_DIR/Gauge.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Raptor"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor.json "$UNITY_TARGET_DIR/raptor.json"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor-pma.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
$sed -i 's/raptor-pma.png/raptor.png/g' "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor-pma.png "$UNITY_TARGET_DIR/raptor.png"


UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Raggedy Spineboy"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy.json "$UNITY_TARGET_DIR/raggedy spineboy.json"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy-pma.atlas "$UNITY_TARGET_DIR/Raggedy Spineboy.atlas.txt"
$sed -i 's/raggedyspineboy-pma.png/Raggedy Spineboy.png/g' "$UNITY_TARGET_DIR/Raggedy Spineboy.atlas.txt"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy-pma.png "$UNITY_TARGET_DIR/Raggedy Spineboy.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-pro"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy-pro.json "$UNITY_TARGET_DIR/spineboy-pro.json"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy-pma.atlas "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
$sed -i 's/spineboy-pma.png/spineboy-pro.png/g' "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy-pma.png "$UNITY_TARGET_DIR/spineboy-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-unity"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy-unity.json "$UNITY_TARGET_DIR/spineboy-unity.json"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy-pma.atlas "$UNITY_TARGET_DIR/spineboy.atlas.txt"
$sed -i 's/spineboy-pma.png/spineboy.png/g' "$UNITY_TARGET_DIR/spineboy.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy-pma.png "$UNITY_TARGET_DIR/spineboy.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Spineunitygirl"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi.json "$UNITY_TARGET_DIR/Doi.json"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi-pma.atlas "$UNITY_TARGET_DIR/Doi.atlas.txt"
$sed -i 's/doi-pma.png/Doi.png/g' "$UNITY_TARGET_DIR/Doi.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi-pma.png "$UNITY_TARGET_DIR/Doi.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Whirlyblendmodes"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes.json "$UNITY_TARGET_DIR/whirlyblendmodes.json"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes-pma.atlas "$UNITY_TARGET_DIR/whirlyblendmodes.atlas.txt"
$sed -i 's/whirlyblendmodes-pma.png/whirlyblendmodes.png/g' "$UNITY_TARGET_DIR/whirlyblendmodes.atlas.txt"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes-pma.png "$UNITY_TARGET_DIR/whirlyblendmodes.png"

echo "--"
echo "Note regarding spine-xna and spine-unity:"
echo "Some textures (normalmap, emission, rim-mask) need manual update."
echo "Please update the following maps when an atlas update is needed:"
echo "[xna and unity]     'raptor/manual-maps' to match png in 'raptor/export/'"
echo "[unity only]   'stretchyman/manual-maps' to match png in 'stretchyman/export/'"
echo "If not updated, the old consistent file-set in the directory is used."
echo "--"
