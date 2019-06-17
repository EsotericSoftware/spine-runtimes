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
if [ -d "$RUNTIME_DIR/spine-libgdx" ]; then
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

	rm -f "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"*
	cp -f ../mix-and-match/export/*.json "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"
	cp -f ../mix-and-match/export/*.skel "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"
	cp -f ../mix-and-match/export/*-pma.* "$RUNTIME_DIR/spine-libgdx/spine-libgdx-tests/assets/mix-and-match/"
else
	echo "skipping spine-libgdx - dir not found"
fi

echo "spine-as3"
if [ -d "$RUNTIME_DIR/spine-as3" ]; then
	rm "$RUNTIME_DIR/spine-as3/spine-as3-example/src/spineboy".*
	cp -f ../spineboy/export/spineboy-ess.json "$RUNTIME_DIR/spine-as3/spine-as3-example/src/"
	cp -f ../spineboy/export/spineboy.atlas "$RUNTIME_DIR/spine-as3/spine-as3-example/src/"
	cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-as3/spine-as3-example/src/"
else
	echo "skipping spine-as3 - dir not found"
fi

echo "spine-cocos2d-objc"
if [ -d "$RUNTIME_DIR/spine-cocos2d-objc" ]; then
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
else
	echo "skipping spine-cocos2d-objc - dir not found"
fi

echo "spine-cocos2dx"
if [ -d "$RUNTIME_DIR/spine-cocos2dx" ]; then
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

	cp -f ../mix-and-match/export/mix-and-match-pro.skel "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
	cp -f ../mix-and-match/export/mix-and-match.atlas "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
	cp -f ../mix-and-match/export/mix-and-match.png "$RUNTIME_DIR/spine-cocos2dx/example/Resources/common/"
else
	echo "skipping spine-cocos2dx - dir not found"
fi

echo "spine-corona"
if [ -d "$RUNTIME_DIR/spine-corona" ]; then
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

	cp -f ../mix-and-match/export/mix-and-match-pro.json "$RUNTIME_DIR/spine-corona/data"
	cp -f ../mix-and-match/export/mix-and-match.atlas "$RUNTIME_DIR/spine-corona/data"
	cp -f ../mix-and-match/export/mix-and-match.png "$RUNTIME_DIR/spine-corona/data"
else
	echo "skipping spine-corona - dir not found"
fi

echo "spine-love"
if [ -d "$RUNTIME_DIR/spine-love" ]; then
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

	cp -f ../mix-and-match/export/mix-and-match-pro.json "$RUNTIME_DIR/spine-love/data"
	cp -f ../mix-and-match/export/mix-and-match.atlas "$RUNTIME_DIR/spine-love/data"
	cp -f ../mix-and-match/export/mix-and-match.png "$RUNTIME_DIR/spine-love/data"
else
	echo "skipping spine-love - dir not found"
fi

echo "spine-sfml-c"
if [ -d "$RUNTIME_DIR/spine-sfml/c" ]; then
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

	cp -f ../mix-and-match/export/mix-and-match-pro.json "$RUNTIME_DIR/spine-sfml/c/data/"
	cp -f ../mix-and-match/export/mix-and-match-pro.skel "$RUNTIME_DIR/spine-sfml/c/data/"
	cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$RUNTIME_DIR/spine-sfml/c/data/"
	cp -f ../mix-and-match/export/mix-and-match-pma.png "$RUNTIME_DIR/spine-sfml/c/data/"
else
	echo "skipping spine-sfml-c - dir not found"
fi

echo "spine-sfml-cpp"
if [ -d "$RUNTIME_DIR/spine-sfml/cpp" ]; then
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

	cp -f ../mix-and-match/export/mix-and-match-pro.json "$RUNTIME_DIR/spine-sfml/cpp/data/"
	cp -f ../mix-and-match/export/mix-and-match-pro.skel "$RUNTIME_DIR/spine-sfml/cpp/data/"
	cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$RUNTIME_DIR/spine-sfml/cpp/data/"
	cp -f ../mix-and-match/export/mix-and-match-pma.png "$RUNTIME_DIR/spine-sfml/cpp/data/"
else
	echo "skipping spine-sfml-cpp - dir not found"
fi

echo "spine-starling"
if [ -d "$RUNTIME_DIR/spine-starling" ]; then
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

	cp -f ../mix-and-match/export/mix-and-match-pro.json "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
	cp -f ../mix-and-match/export/mix-and-match.atlas "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
	cp -f ../mix-and-match/export/mix-and-match.png "$RUNTIME_DIR/spine-starling/spine-starling-example/src/"
else
	echo "skipping spine-starling - dir not found"
fi

echo "spine-ts"
if [ -d "$RUNTIME_DIR/spine-ts" ]; then
	rm "$RUNTIME_DIR/spine-ts/webgl/example/assets/"*
	cp -f ../coin/export/coin-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../coin/export/coin-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../coin/export/coin-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../goblins/export/goblins-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../goblins/export/goblins-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../goblins/export/goblins-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../raptor/export/raptor-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../raptor/export/raptor-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../raptor/export/raptor-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../spineboy/export/spineboy-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../spineboy/export/spineboy-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../spineboy/export/spineboy-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../spineboy/export/spineboy.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../tank/export/tank-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../tank/export/tank-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../tank/export/tank-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../vine/export/vine-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../vine/export/vine-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../vine/export/vine-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../owl/export/owl-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../owl/export/owl-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../owl/export/owl-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../stretchyman/export/stretchyman-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../stretchyman/export/stretchyman-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../stretchyman/export/stretchyman-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../stretchyman/export/stretchyman-stretchy-ik-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

	cp -f ../mix-and-match/export/mix-and-match-pro.skel "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../mix-and-match/export/mix-and-match-pma.atlas "$RUNTIME_DIR/spine-ts/webgl/example/assets/"
	cp -f ../mix-and-match/export/mix-and-match-pma.png "$RUNTIME_DIR/spine-ts/webgl/example/assets/"

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

	cp -f ../spineboy/export/spineboy-pro.skel "$RUNTIME_DIR/spine-ts/player/example/assets/"
	cp -f ../spineboy/export/spineboy-pma.atlas "$RUNTIME_DIR/spine-ts/player/example/assets/"
	cp -f ../spineboy/export/spineboy-pma.png "$RUNTIME_DIR/spine-ts/player/example/assets/"
else
	echo "skipping spine-ts - dir not found"
fi

echo "spine-xna"
if [ -d "$RUNTIME_DIR/spine-xna" ]; then
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
else
	echo "skipping spine-xna - dir not found"
fi

echo "spine-unity"
if [ -d "$RUNTIME_DIR/spine-unity" ]; then
	# Section of assets specific for the spine-unity runtime
	UNITY_SPECIFIC_ASSETS_SOURCE_DIR=../spine-unity

	# DO NOT DELETE EVERYTHING IN UNITY DIRS, ESPECIALLY NOT .meta FILES.
	# Note: We copy the files following the existing naming scheme (e.g. goblins.json instead of goblins-pro.json)
	#       to the unity assets directories. This requires to change the png file reference line in the atlas file.
	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Dragon"
	cp -f ../dragon/export/dragon-ess.json "$UNITY_ASSET_TARGET_DIR/dragon.json"
	cp -f ../dragon/export/dragon-pma.atlas "$UNITY_ASSET_TARGET_DIR/dragon.atlas.txt"
	sed -i 's/dragon-pma.png/dragon.png/g' "$UNITY_ASSET_TARGET_DIR/dragon.atlas.txt"
	sed -i 's/dragon-pma2.png/dragon2.png/g' "$UNITY_ASSET_TARGET_DIR/dragon.atlas.txt"
	cp -f ../dragon/export/dragon-pma.png "$UNITY_ASSET_TARGET_DIR/dragon.png"
	cp -f ../dragon/export/dragon-pma2.png "$UNITY_ASSET_TARGET_DIR/dragon2.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Goblins"
	cp -f ../goblins/export/goblins-pro.json "$UNITY_ASSET_TARGET_DIR/goblins.json"
	cp -f ../goblins/export/goblins-pma.atlas "$UNITY_ASSET_TARGET_DIR/goblins.atlas.txt"
	sed -i 's/goblins-pma.png/goblins.png/g' "$UNITY_ASSET_TARGET_DIR/goblins.atlas.txt"
	cp -f ../goblins/export/goblins-pma.png "$UNITY_ASSET_TARGET_DIR/goblins.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Hero"
	cp -f ../spine-unity/hero/export/hero-pro.json "$UNITY_ASSET_TARGET_DIR/"
	cp -f ../spine-unity/hero/export/hero-pma.atlas "$UNITY_ASSET_TARGET_DIR/hero-pro.atlas.txt"
	sed -i 's/hero-pma.png/hero-pro.png/g' "$UNITY_ASSET_TARGET_DIR/hero-pro.atlas.txt"
	cp -f ../spine-unity/hero/export/hero-pma.png "$UNITY_ASSET_TARGET_DIR/hero-pro.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Raptor"
	cp -f ../raptor/export/raptor-pro.json "$UNITY_ASSET_TARGET_DIR/raptor.json"
	cp -f ../raptor/export/raptor-pma.atlas "$UNITY_ASSET_TARGET_DIR/raptor.atlas.txt"
	sed -i 's/raptor-pma.png/raptor.png/g' "$UNITY_ASSET_TARGET_DIR/raptor.atlas.txt"
	cp -f ../raptor/export/raptor-pma.png "$UNITY_ASSET_TARGET_DIR/raptor.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-pro"
	cp -f ../spineboy/export/spineboy-pro.json "$UNITY_ASSET_TARGET_DIR/spineboy-pro.json"
	cp -f ../spineboy/export/spineboy-pma.atlas "$UNITY_ASSET_TARGET_DIR/spineboy-pro.atlas.txt"
	sed -i 's/spineboy-pma.png/spineboy-pro.png/g' "$UNITY_ASSET_TARGET_DIR/spineboy-pro.atlas.txt"
	cp -f ../spineboy/export/spineboy-pma.png "$UNITY_ASSET_TARGET_DIR/spineboy-pro.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Stretchyman"
	cp -f ../stretchyman/export/stretchyman-pro.json "$UNITY_ASSET_TARGET_DIR/stretchyman.json"
	cp -f ../stretchyman/export/stretchyman-pma.atlas "$UNITY_ASSET_TARGET_DIR/stretchyman-diffuse-pma.atlas.txt"
	sed -i 's/stretchyman-pma.png/stretchyman-diffuse-pma.png/g' "$UNITY_ASSET_TARGET_DIR/stretchyman-diffuse-pma.atlas.txt"
	cp -f ../stretchyman/export/stretchyman-pma.png "$UNITY_ASSET_TARGET_DIR/stretchyman-diffuse-pma.png"
	# Note: normalmap and emissionmap have been created manually, a recreated version is copied to the target dir.
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/stretchyman/stretchyman-normals.png "$UNITY_ASSET_TARGET_DIR/"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/stretchyman/stretchyman-emission.png "$UNITY_ASSET_TARGET_DIR/"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Eyes"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/eyes/export/eyes.json "$UNITY_ASSET_TARGET_DIR/eyes.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/eyes/export/eyes-pma.atlas "$UNITY_ASSET_TARGET_DIR/eyes.atlas.txt"
	sed -i 's/eyes-pma.png/eyes.png/g' "$UNITY_ASSET_TARGET_DIR/eyes.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/eyes/export/eyes-pma.png "$UNITY_ASSET_TARGET_DIR/eyes.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/FootSoldier"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/footsoldier/export/footsoldier.json "$UNITY_ASSET_TARGET_DIR/FootSoldier.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/footsoldier/export/footsoldier-pma.atlas "$UNITY_ASSET_TARGET_DIR/FS_White.atlas.txt"
	sed -i 's/footsoldier-pma.png/FS_White.png/g' "$UNITY_ASSET_TARGET_DIR/FS_White.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/footsoldier/export/footsoldier-pma.png "$UNITY_ASSET_TARGET_DIR/FS_White.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Gauge"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/gauge/export/gauge.json "$UNITY_ASSET_TARGET_DIR/Gauge.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/gauge/export/gauge-pma.atlas "$UNITY_ASSET_TARGET_DIR/Gauge.atlas.txt"
	sed -i 's/gauge-pma.png/Gauge.png/g' "$UNITY_ASSET_TARGET_DIR/Gauge.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/gauge/export/gauge-pma.png "$UNITY_ASSET_TARGET_DIR/Gauge.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Raptor"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/raptor/export/raptor.json "$UNITY_ASSET_TARGET_DIR/raptor.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/raptor/export/raptor-pma.atlas "$UNITY_ASSET_TARGET_DIR/raptor.atlas.txt"
	sed -i 's/raptor-pma.png/raptor.png/g' "$UNITY_ASSET_TARGET_DIR/raptor.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/raptor/export/raptor-pma.png "$UNITY_ASSET_TARGET_DIR/raptor.png"


	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Raggedy Spineboy"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy.json "$UNITY_ASSET_TARGET_DIR/raggedy spineboy.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy-pma.atlas "$UNITY_ASSET_TARGET_DIR/Raggedy Spineboy.atlas.txt"
	sed -i 's/raggedyspineboy-pma.png/Raggedy Spineboy.png/g' "$UNITY_ASSET_TARGET_DIR/Raggedy Spineboy.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/raggedyspineboy/export/raggedyspineboy-pma.png "$UNITY_ASSET_TARGET_DIR/Raggedy Spineboy.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-pro"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineboy-pro/export/spineboy-pro.json "$UNITY_ASSET_TARGET_DIR/spineboy-pro.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineboy-pro/export/spineboy-pma.atlas "$UNITY_ASSET_TARGET_DIR/spineboy-pro.atlas.txt"
	sed -i 's/spineboy-pma.png/spineboy-pro.png/g' "$UNITY_ASSET_TARGET_DIR/spineboy-pro.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineboy-pro/export/spineboy-pma.png "$UNITY_ASSET_TARGET_DIR/spineboy-pro.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/spineboy-unity"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineboy-unity/export/spineboy-unity.json "$UNITY_ASSET_TARGET_DIR/spineboy-unity.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineboy-unity/export/spineboy-pma.atlas "$UNITY_ASSET_TARGET_DIR/spineboy.atlas.txt"
	sed -i 's/spineboy-pma.png/spineboy.png/g' "$UNITY_ASSET_TARGET_DIR/spineboy.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineboy-unity/export/spineboy-pma.png "$UNITY_ASSET_TARGET_DIR/spineboy.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Spineunitygirl"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineunitygirl/export/doi.json "$UNITY_ASSET_TARGET_DIR/Doi.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineunitygirl/export/doi-pma.atlas "$UNITY_ASSET_TARGET_DIR/Doi.atlas.txt"
	sed -i 's/doi-pma.png/Doi.png/g' "$UNITY_ASSET_TARGET_DIR/Doi.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/spineunitygirl/export/doi-pma.png "$UNITY_ASSET_TARGET_DIR/Doi.png"

	UNITY_ASSET_TARGET_DIR="$RUNTIME_DIR/spine-unity/Assets/Spine Examples/Spine Skeletons/Whirlyblendmodes"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes.json "$UNITY_ASSET_TARGET_DIR/whirlyblendmodes.json"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes-pma.atlas "$UNITY_ASSET_TARGET_DIR/whirlyblendmodes.atlas.txt"
	sed -i 's/whirlyblendmodes-pma.png/whirlyblendmodes.png/g' "$UNITY_ASSET_TARGET_DIR/whirlyblendmodes.atlas.txt"
	cp -f $UNITY_SPECIFIC_ASSETS_SOURCE_DIR/whirlyblendmodes/export/whirlyblendmodes-pma.png "$UNITY_ASSET_TARGET_DIR/whirlyblendmodes.png"
else
	echo "skipping spine-unity - dir not found"
fi
