#!/bin/sh
set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
cd $SCRIPT_DIR

ROOT=${1%/}
if [ ! -d "$ROOT/" ]; then
	echo "Please provide the path to the Spine Runtimes root directory."
	exit -1
fi
if [ ! -f "$ROOT/CHANGELOG.md" ]; then
	echo "Provided path does not look like the Spine Runtimes root directory: $ROOT"
	exit -1
fi
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

echo "spine-as3"
rm "$ROOT/spine-as3/spine-as3-example/src/spineboy".*
cp -f ../spineboy/export/spineboy-ess.json "$ROOT/spine-as3/spine-as3-example/src/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-as3/spine-as3-example/src/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-as3/spine-as3-example/src/"

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
rm "$ROOT/spine-cocos2dx/example/Resources/common/"*

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


echo "spine-corona"
rm "$ROOT/spine-corona/data/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-corona/data"
cp -f ../coin/export/coin.atlas "$ROOT/spine-corona/data"
cp -f ../coin/export/coin.png "$ROOT/spine-corona/data"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-corona/data"
cp -f ../goblins/export/goblins.atlas "$ROOT/spine-corona/data"
cp -f ../goblins/export/goblins.png "$ROOT/spine-corona/data"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-corona/data"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-corona/data"
cp -f ../raptor/export/raptor.png "$ROOT/spine-corona/data"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-corona/data"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-corona/data"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-corona/data"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-corona/data"
cp -f ../tank/export/tank.atlas "$ROOT/spine-corona/data"
cp -f ../tank/export/tank.png "$ROOT/spine-corona/data"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-corona/data"
cp -f ../vine/export/vine.atlas "$ROOT/spine-corona/data"
cp -f ../vine/export/vine.png "$ROOT/spine-corona/data"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-corona/data"
cp -f ../stretchyman/export/stretchyman.atlas "$ROOT/spine-corona/data"
cp -f ../stretchyman/export/stretchyman.png "$ROOT/spine-corona/data"

cp -f ../owl/export/owl-pro.json "$ROOT/spine-corona/data"
cp -f ../owl/export/owl.atlas "$ROOT/spine-corona/data"
cp -f ../owl/export/owl.png "$ROOT/spine-corona/data"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-corona/data"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-corona/data"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-corona/data"

echo "spine-love"
rm "$ROOT/spine-love/data/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-love/data"
cp -f ../coin/export/coin.atlas "$ROOT/spine-love/data"
cp -f ../coin/export/coin.png "$ROOT/spine-love/data"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-love/data"
cp -f ../goblins/export/goblins.atlas "$ROOT/spine-love/data"
cp -f ../goblins/export/goblins.png "$ROOT/spine-love/data"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-love/data"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-love/data"
cp -f ../raptor/export/raptor.png "$ROOT/spine-love/data"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-love/data"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-love/data"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-love/data"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-love/data"
cp -f ../tank/export/tank.atlas "$ROOT/spine-love/data"
cp -f ../tank/export/tank.png "$ROOT/spine-love/data"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-love/data"
cp -f ../vine/export/vine.atlas "$ROOT/spine-love/data"
cp -f ../vine/export/vine.png "$ROOT/spine-love/data"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-love/data"
cp -f ../stretchyman/export/stretchyman.atlas "$ROOT/spine-love/data"
cp -f ../stretchyman/export/stretchyman.png "$ROOT/spine-love/data"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-love/data"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-love/data"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-love/data"

echo "spine-sfml-c"
rm "$ROOT/spine-sfml/c/data/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-sfml/c/data/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-sfml/c/data/"
cp -f ../coin/export/coin-pma.atlas "$ROOT/spine-sfml/c/data/"
cp -f ../coin/export/coin-pma.png "$ROOT/spine-sfml/c/data/"

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

echo "spine-starling"

# Do not delete everything in src, especially goblins-mesh-starling.png/.xml
cp -f ../coin/export/coin-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../coin/export/coin.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../coin/export/coin.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../goblins/export/goblins-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../goblins/export/goblins.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../goblins/export/goblins.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../spineboy/export/spineboy-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../tank/export/tank.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../tank/export/tank.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../vine/export/vine-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../vine/export/vine.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../vine/export/vine.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../stretchyman/export/stretchyman-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../stretchyman/export/stretchyman.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../stretchyman/export/stretchyman.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../owl/export/owl-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../owl/export/owl.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../owl/export/owl.png "$ROOT/spine-starling/spine-starling-example/src/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-starling/spine-starling-example/src/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-starling/spine-starling-example/src/"

echo "spine-ts"
rm "$ROOT/spine-ts/webgl/example/assets/"*
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../coin/export/coin-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../coin/export/coin-pma.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../goblins/export/goblins-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../goblins/export/goblins-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../goblins/export/goblins-pma.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../raptor/export/raptor-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../raptor/export/raptor-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../raptor/export/raptor-pma.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../tank/export/tank-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../tank/export/tank-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../tank/export/tank-pma.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../vine/export/vine-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../vine/export/vine-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../vine/export/vine-pma.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../owl/export/owl-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../owl/export/owl-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../owl/export/owl-pma.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../stretchyman/export/stretchyman-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../stretchyman/export/stretchyman-pma.png "$ROOT/spine-ts/webgl/example/assets/"

cp -f ../mix-and-match/export/mix-and-match-pro.skel "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$ROOT/spine-ts/webgl/example/assets/"
cp -f ../mix-and-match/export/mix-and-match-pma.png "$ROOT/spine-ts/webgl/example/assets/"

rm "$ROOT/spine-ts/canvas/example/assets/"*
cp -f ../spineboy/export/spineboy-ess.json "$ROOT/spine-ts/canvas/example/assets/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-ts/canvas/example/assets/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-ts/canvas/example/assets/"

rm "$ROOT/spine-ts/threejs/example/assets/"*
cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ts/threejs/example/assets/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-ts/threejs/example/assets/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-ts/threejs/example/assets/"

rm "$ROOT/spine-ts/player/example/assets/"*
cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-ts/player/example/assets/"
cp -f ../raptor/export/raptor-pma.atlas "$ROOT/spine-ts/player/example/assets/"
cp -f ../raptor/export/raptor-pma.png "$ROOT/spine-ts/player/example/assets/"

cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-ts/player/example/assets/"
cp -f ../spineboy/export/spineboy-pma.atlas "$ROOT/spine-ts/player/example/assets/"
cp -f ../spineboy/export/spineboy-pma.png "$ROOT/spine-ts/player/example/assets/"

echo "spine-xna"
rm "$ROOT/spine-xna/example/data/"*
cp -f ../coin/export/coin-pro.json "$ROOT/spine-xna/example/data/"
cp -f ../coin/export/coin-pro.skel "$ROOT/spine-xna/example/data/"
cp -f ../coin/export/coin.atlas "$ROOT/spine-xna/example/data/"
cp -f ../coin/export/coin.png "$ROOT/spine-xna/example/data/"

cp -f ../raptor/export/raptor-pro.json "$ROOT/spine-xna/example/data/"
cp -f ../raptor/export/raptor.atlas "$ROOT/spine-xna/example/data/"
cp -f ../raptor/export/raptor.png "$ROOT/spine-xna/example/data/"

cp -f ../spineboy/export/spineboy-pro.skel "$ROOT/spine-xna/example/data/"
cp -f ../spineboy/export/spineboy.atlas "$ROOT/spine-xna/example/data/"
cp -f ../spineboy/export/spineboy.png "$ROOT/spine-xna/example/data/"

cp -f ../tank/export/tank-pro.json "$ROOT/spine-xna/example/data/"
cp -f ../tank/export/tank.atlas "$ROOT/spine-xna/example/data/"
cp -f ../tank/export/tank.png "$ROOT/spine-xna/example/data/"

cp -f ../mix-and-match/export/mix-and-match-pro.json "$ROOT/spine-xna/example/data/"
cp -f ../mix-and-match/export/mix-and-match.atlas "$ROOT/spine-xna/example/data/"
cp -f ../mix-and-match/export/mix-and-match.png "$ROOT/spine-xna/example/data/"

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

# Do not delete everything in unity dirs, especially not .meta files.
# Note: We copy the files following the existing naming scheme (e.g. goblins.json instead of goblins-pro.json)
#       to the unity assets directories. This requires to change the png file reference line in the atlas file.
UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Dragon"
cp -f ../dragon/export/dragon-ess.json "$UNITY_TARGET_DIR/dragon.json"
cp -f ../dragon/export/dragon-pma.atlas "$UNITY_TARGET_DIR/dragon.atlas.txt"
sed -i 's/dragon-pma.png/dragon.png/g' "$UNITY_TARGET_DIR/dragon.atlas.txt"
sed -i 's/dragon-pma2.png/dragon2.png/g' "$UNITY_TARGET_DIR/dragon.atlas.txt"
cp -f ../dragon/export/dragon-pma.png "$UNITY_TARGET_DIR/dragon.png"
cp -f ../dragon/export/dragon-pma2.png "$UNITY_TARGET_DIR/dragon2.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Goblins"
cp -f ../goblins/export/goblins-pro.json "$UNITY_TARGET_DIR/goblins.json"
cp -f ../goblins/export/goblins-pma.atlas "$UNITY_TARGET_DIR/goblins.atlas.txt"
sed -i 's/goblins-pma.png/goblins.png/g' "$UNITY_TARGET_DIR/goblins.atlas.txt"
cp -f ../goblins/export/goblins-pma.png "$UNITY_TARGET_DIR/goblins.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Hero"
cp -f ../hero/export/hero-pro.json "$UNITY_TARGET_DIR/"
cp -f ../hero/export/hero-pma.atlas "$UNITY_TARGET_DIR/hero-pro.atlas.txt"
sed -i 's/hero-pma.png/hero-pro.png/g' "$UNITY_TARGET_DIR/hero-pro.atlas.txt"
cp -f ../hero/export/hero-pma.png "$UNITY_TARGET_DIR/hero-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Raptor"
cp -f ../raptor/export/raptor-pro.json "$UNITY_TARGET_DIR/raptor.json"
cp -f ../raptor/export/raptor-pma.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
sed -i 's/raptor-pma.png/raptor.png/g' "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f ../raptor/export/raptor-pma.png "$UNITY_TARGET_DIR/raptor.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-pro"
cp -f ../spineboy/export/spineboy-pro.json "$UNITY_TARGET_DIR/spineboy-pro.json"
cp -f ../spineboy/export/spineboy-pma.atlas "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
sed -i 's/spineboy-pma.png/spineboy-pro.png/g' "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
cp -f ../spineboy/export/spineboy-pma.png "$UNITY_TARGET_DIR/spineboy-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Stretchyman"
cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_TARGET_DIR/stretchyman.json"
cp -f ../stretchyman/export/stretchyman-pma.atlas "$UNITY_TARGET_DIR/stretchyman-diffuse-pma.atlas.txt"
sed -i 's/stretchyman-pma.png/stretchyman-diffuse-pma.png/g' "$UNITY_TARGET_DIR/stretchyman-diffuse-pma.atlas.txt"
cp -f ../stretchyman/export/stretchyman-pma.png "$UNITY_TARGET_DIR/stretchyman-diffuse-pma.png"
# Note: normalmap and emissionmap have been created manually, a recreated version is copied to the target dir.
cp -f $UNITY_SOURCE_DIR/stretchyman/stretchyman-normals.png "$UNITY_TARGET_DIR/"
cp -f $UNITY_SOURCE_DIR/stretchyman/stretchyman-emission.png "$UNITY_TARGET_DIR/"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Eyes"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes.json "$UNITY_TARGET_DIR/eyes.json"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes-pma.atlas "$UNITY_TARGET_DIR/eyes.atlas.txt"
sed -i 's/eyes-pma.png/eyes.png/g' "$UNITY_TARGET_DIR/eyes.atlas.txt"
cp -f $UNITY_SOURCE_DIR/eyes/export/eyes-pma.png "$UNITY_TARGET_DIR/eyes.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/FootSoldier"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier.json "$UNITY_TARGET_DIR/FootSoldier.json"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier-pma.atlas "$UNITY_TARGET_DIR/FS_White.atlas.txt"
sed -i 's/footsoldier-pma.png/FS_White.png/g' "$UNITY_TARGET_DIR/FS_White.atlas.txt"
cp -f $UNITY_SOURCE_DIR/footsoldier/export/footsoldier-pma.png "$UNITY_TARGET_DIR/FS_White.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Gauge"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge.json "$UNITY_TARGET_DIR/Gauge.json"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge-pma.atlas "$UNITY_TARGET_DIR/Gauge.atlas.txt"
sed -i 's/gauge-pma.png/Gauge.png/g' "$UNITY_TARGET_DIR/Gauge.atlas.txt"
cp -f $UNITY_SOURCE_DIR/gauge/export/gauge-pma.png "$UNITY_TARGET_DIR/Gauge.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Raptor"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor.json "$UNITY_TARGET_DIR/raptor.json"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor-pma.atlas "$UNITY_TARGET_DIR/raptor.atlas.txt"
sed -i 's/raptor-pma.png/raptor.png/g' "$UNITY_TARGET_DIR/raptor.atlas.txt"
cp -f $UNITY_SOURCE_DIR/raptor/export/raptor-pma.png "$UNITY_TARGET_DIR/raptor.png"


UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Raggedy Spineboy"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy.json "$UNITY_TARGET_DIR/raggedy spineboy.json"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy-pma.atlas "$UNITY_TARGET_DIR/Raggedy Spineboy.atlas.txt"
sed -i 's/raggedyspineboy-pma.png/Raggedy Spineboy.png/g' "$UNITY_TARGET_DIR/Raggedy Spineboy.atlas.txt"
cp -f $UNITY_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy-pma.png "$UNITY_TARGET_DIR/Raggedy Spineboy.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-pro"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy-pro.json "$UNITY_TARGET_DIR/spineboy-pro.json"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy-pma.atlas "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
sed -i 's/spineboy-pma.png/spineboy-pro.png/g' "$UNITY_TARGET_DIR/spineboy-pro.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineboy-pro/export/spineboy-pma.png "$UNITY_TARGET_DIR/spineboy-pro.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-unity"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy-unity.json "$UNITY_TARGET_DIR/spineboy-unity.json"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy-pma.atlas "$UNITY_TARGET_DIR/spineboy.atlas.txt"
sed -i 's/spineboy-pma.png/spineboy.png/g' "$UNITY_TARGET_DIR/spineboy.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineboy-unity/export/spineboy-pma.png "$UNITY_TARGET_DIR/spineboy.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Spineunitygirl"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi.json "$UNITY_TARGET_DIR/Doi.json"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi-pma.atlas "$UNITY_TARGET_DIR/Doi.atlas.txt"
sed -i 's/doi-pma.png/Doi.png/g' "$UNITY_TARGET_DIR/Doi.atlas.txt"
cp -f $UNITY_SOURCE_DIR/spineunitygirl/export/doi-pma.png "$UNITY_TARGET_DIR/Doi.png"

UNITY_TARGET_DIR="$ROOT/spine-unity/Assets/Spine Examples/Spine Skeletons/Whirlyblendmodes"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes.json "$UNITY_TARGET_DIR/whirlyblendmodes.json"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes-pma.atlas "$UNITY_TARGET_DIR/whirlyblendmodes.atlas.txt"
sed -i 's/whirlyblendmodes-pma.png/whirlyblendmodes.png/g' "$UNITY_TARGET_DIR/whirlyblendmodes.atlas.txt"
cp -f $UNITY_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes-pma.png "$UNITY_TARGET_DIR/whirlyblendmodes.png"
