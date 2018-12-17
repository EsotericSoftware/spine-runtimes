#!/bin/sh
set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
cd $SCRIPT_DIR

RUNTIME_DIR=${1%/}
if [ ! -d "$RUNTIME_DIR/" ]; then
	echo "Please provide the path to the spine-runtimes/ directory."
	exit -1
fi
if [ ! -f "$RUNTIME_DIR/CHANGELOG.md" ]; then
	echo "Provided runtime directory $RUNTIME_DIR is not a spine-runtimes/ directory."
	exit -1
fi
echo "Runtime directory: $RUNTIME_DIR"
echo "Copying assets to runtimes ..."

echo ""
echo "spine-libgdx"
rm "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/goblins/"*
cp -f ../goblins/export/*.json "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/goblins/"
cp -f ../goblins/export/*.skel "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/goblins/"
cp -f ../goblins/export/*-pma.* "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/goblins/"

rm "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/raptor/"*
cp -f ../raptor/export/*.json "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/raptor/"
cp -f ../raptor/export/*.skel "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/raptor/"
cp -f ../raptor/export/*-pma.* "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/raptor/"

rm "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/spineboy/"*
cp -f ../spineboy/export/*.json "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/spineboy/"
cp -r ../spineboy/export/*.skel "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/spineboy/"
cp -r ../spineboy/export/*-pma.* "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/spineboy/"

rm "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/coin/"*
cp -f ../coin/export/*.json "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/coin/"
cp -f ../coin/export/*.skel "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/coin/"
cp -f ../coin/export/*-pma.* "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/coin/"

echo "spine-as3"
rm "$RUNTIME_DIR/spine-as3/spine-as3-example/src/spineboy".*
cp -f ../spineboy/export/spineboy-ess.json "$RUNTIME_DIR/spine-as3/spine-as3-example/src/"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-as3/spine-as3-example/src/"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-as3/spine-as3-example/src/"

echo "spine-cocos2d-objc"
rm "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"*

cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../coin/export/coin.atlas "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../coin/export/coin.png "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../goblins/export/goblins.atlas "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../goblins/export/goblins.png "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../raptor/export/raptor.atlas "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../raptor/export/raptor.png "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"

cp -f ../spineboy/export/spineboy-ess.json "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../tank/export/tank.atlas "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"
cp -f ../tank/export/tank.png "$RUNTIME_DIR/spine-cocos2d-objc/Resources/"

echo "spine-cocos2dx"
rm "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"*

cp -f ../coin/export/coin-pro.skel "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../coin/export/coin.atlas "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../coin/export/coin.png "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../goblins/export/goblins.atlas "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../goblins/export/goblins.png "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../raptor/export/raptor.atlas "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../raptor/export/raptor.png "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"

cp -f ../tank/export/tank-pro.skel "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../tank/export/tank.atlas "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
cp -f ../tank/export/tank.png "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"

echo "spine-corona"
rm "$RUNTIME_DIR/spine-corona/data/"*
cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../coin/export/coin.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../coin/export/coin.png "$RUNTIME_DIR/spine-corona/data"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../goblins/export/goblins.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../goblins/export/goblins.png "$RUNTIME_DIR/spine-corona/data"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../raptor/export/raptor.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../raptor/export/raptor.png "$RUNTIME_DIR/spine-corona/data"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-corona/data"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../tank/export/tank.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../tank/export/tank.png "$RUNTIME_DIR/spine-corona/data"

cp -f ../vine/export/vine-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../vine/export/vine.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../vine/export/vine.png "$RUNTIME_DIR/spine-corona/data"

cp -f ../stretchyman/export/stretchyman-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../stretchyman/export/stretchyman.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../stretchyman/export/stretchyman.png "$RUNTIME_DIR/spine-corona/data"

cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.json "$RUNTIME_DIR/spine-corona/data"

cp -f ../owl/export/owl-pro.json "$RUNTIME_DIR/spine-corona/data"
cp -f ../owl/export/owl.atlas "$RUNTIME_DIR/spine-corona/data"
cp -f ../owl/export/owl.png "$RUNTIME_DIR/spine-corona/data"

echo "spine-love"
rm "$RUNTIME_DIR/spine-love/data/"*
cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-love/data"
cp -f ../coin/export/coin.atlas "$RUNTIME_DIR/spine-love/data"
cp -f ../coin/export/coin.png "$RUNTIME_DIR/spine-love/data"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-love/data"
cp -f ../goblins/export/goblins.atlas "$RUNTIME_DIR/spine-love/data"
cp -f ../goblins/export/goblins.png "$RUNTIME_DIR/spine-love/data"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-love/data"
cp -f ../raptor/export/raptor.atlas "$RUNTIME_DIR/spine-love/data"
cp -f ../raptor/export/raptor.png "$RUNTIME_DIR/spine-love/data"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-love/data"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-love/data"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-love/data"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-love/data"
cp -f ../tank/export/tank.atlas "$RUNTIME_DIR/spine-love/data"
cp -f ../tank/export/tank.png "$RUNTIME_DIR/spine-love/data"

cp -f ../vine/export/vine-pro.json "$RUNTIME_DIR/spine-love/data"
cp -f ../vine/export/vine.atlas "$RUNTIME_DIR/spine-love/data"
cp -f ../vine/export/vine.png "$RUNTIME_DIR/spine-love/data"

cp -f ../stretchyman/export/stretchyman-pro.json "$RUNTIME_DIR/spine-love/data"
cp -f ../stretchyman/export/stretchyman.atlas "$RUNTIME_DIR/spine-love/data"
cp -f ../stretchyman/export/stretchyman.png "$RUNTIME_DIR/spine-love/data"

cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.json "$RUNTIME_DIR/spine-love/data"

echo "spine-sfml-c"
rm "$RUNTIME_DIR/spine-sfml/c/data/"*
cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../coin/export/coin-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../coin/export/coin-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../coin/export/coin-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../goblins/export/goblins-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../goblins/export/goblins-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../goblins/export/goblins-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../raptor/export/raptor-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../raptor/export/raptor-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../raptor/export/raptor-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../spineboy/export/spineboy-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../spineboy/export/spineboy-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../spineboy/export/spineboy-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../tank/export/tank-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../tank/export/tank-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../tank/export/tank-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

cp -f ../vine/export/vine-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../vine/export/vine-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../vine/export/vine-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../vine/export/vine-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

cp -f ../stretchyman/export/stretchyman-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../stretchyman/export/stretchyman-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../stretchyman/export/stretchyman-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.json "$RUNTIME_DIR/spine-sfml/c/data"
cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.skel "$RUNTIME_DIR/spine-sfml/c/data"

cp -f ../owl/export/owl-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../owl/export/owl-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../owl/export/owl-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
cp -f ../owl/export/owl-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"

echo "spine-sfml-cpp"
rm "$RUNTIME_DIR/spine-sfml/cpp/data/"*
cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../coin/export/coin-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../coin/export/coin-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../coin/export/coin-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../goblins/export/goblins-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../goblins/export/goblins-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../goblins/export/goblins-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../raptor/export/raptor-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../raptor/export/raptor-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../raptor/export/raptor-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../spineboy/export/spineboy-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../spineboy/export/spineboy-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../spineboy/export/spineboy-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../tank/export/tank-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../tank/export/tank-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../tank/export/tank-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

cp -f ../vine/export/vine-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../vine/export/vine-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../vine/export/vine-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../vine/export/vine-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

cp -f ../stretchyman/export/stretchyman-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../stretchyman/export/stretchyman-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../stretchyman/export/stretchyman-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data"
cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data"

cp -f ../owl/export/owl-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../owl/export/owl-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../owl/export/owl-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
cp -f ../owl/export/owl-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"

echo "spine-starling"
# DO NOT DELETE EVERYTHING IN SOURCE, ESPECIALLY goblins-mesh-starling.png/.xml
cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../coin/export/coin.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../coin/export/coin.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../goblins/export/goblins.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../goblins/export/goblins.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../raptor/export/raptor.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../raptor/export/raptor.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../tank/export/tank.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../tank/export/tank.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../vine/export/vine-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../vine/export/vine.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../vine/export/vine.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../stretchyman/export/stretchyman-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../stretchyman/export/stretchyman.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../stretchyman/export/stretchyman.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

cp -f ../owl/export/owl-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../owl/export/owl.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
cp -f ../owl/export/owl.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"

echo "spine-ts"
rm "$RUNTIME_DIR/spine-ts/webgl/example/assets/"*
cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../coin/export/coin-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../coin/export/coin-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../goblins/export/goblins-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../goblins/export/goblins-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../raptor/export/raptor-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../raptor/export/raptor-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../tank/export/tank-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../tank/export/tank-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../vine/export/vine-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../vine/export/vine-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../vine/export/vine-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../owl/export/owl-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../owl/export/owl-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../owl/export/owl-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../stretchyman/export/stretchyman-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
cp -f ../stretchyman/export/stretchyman-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.json "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

rm "$RUNTIME_DIR/spine-ts/canvas/example/assets/"*
cp -f ../spineboy/export/spineboy-ess.json "$RUNTIME_DIR/spine-ts/canvas/example/assets/"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-ts/canvas/example/assets/"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-ts/canvas/example/assets/"

rm "$RUNTIME_DIR/spine-ts/threejs/example/assets/"*
cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-ts/threejs/example/assets/"
cp -f ../raptor/export/raptor.atlas "$RUNTIME_DIR/spine-ts/threejs/example/assets/"
cp -f ../raptor/export/raptor.png "$RUNTIME_DIR/spine-ts/threejs/example/assets/"

rm "$RUNTIME_DIR/spine-ts/player/example/assets/"*
cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-ts/player/example/assets/"
cp -f ../raptor/export/raptor-pma.atlas "$RUNTIME_DIR/spine-ts/player/example/assets/"
cp -f ../raptor/export/raptor-pma.png "$RUNTIME_DIR/spine-ts/player/example/assets/"

cp -f ../spineboy/export/spineboy-pro.json "$RUNTIME_DIR/spine-ts/player/example/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$RUNTIME_DIR/spine-ts/player/example/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$RUNTIME_DIR/spine-ts/player/example/assets/"

echo "spine-xna"
rm "$RUNTIME_DIR/spine-xna/example/data/"*
cp -f ../coin/export/coin-pro.json "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../coin/export/coin-pro.skel "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../coin/export/coin.atlas "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../coin/export/coin.png "$RUNTIME_DIR/spine-xna/example/data/"

cp -f ../goblins/export/goblins-pro.json "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../goblins/export/goblins-pro.skel "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../goblins/export/goblins.atlas "$RUNTIME_DIR/spine-xna/example/data/goblins-mesh.atlas"
cp -f ../goblins/export/goblins.png "$RUNTIME_DIR/spine-xna/example/data/"

cp -f ../raptor/export/raptor-pro.json "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../raptor/export/raptor-pro.skel "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../raptor/export/raptor.atlas "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../raptor/export/raptor.png "$RUNTIME_DIR/spine-xna/example/data/"

cp -f ../spineboy/export/spineboy-ess.json "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../spineboy/export/spineboy-ess.skel "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-xna/example/data/"

cp -f ../tank/export/tank-pro.json "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../tank/export/tank-pro.skel "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../tank/export/tank.atlas "$RUNTIME_DIR/spine-xna/example/data/"
cp -f ../tank/export/tank.png "$RUNTIME_DIR/spine-xna/example/data/"