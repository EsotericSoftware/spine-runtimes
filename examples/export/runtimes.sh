#!/bin/sh
set -e
echo "Copying assets to runtimes..."

echo ""
echo "spine-libgdx"
rm -rf ../../spine-libgdx/spine-libgdx-tests/assets/goblins/*
cp -f ../goblins/export/*.json ../../spine-libgdx/spine-libgdx-tests/assets/goblins/
cp -f ../goblins/export/*.skel ../../spine-libgdx/spine-libgdx-tests/assets/goblins/
cp -f ../goblins/export/*-pma.* ../../spine-libgdx/spine-libgdx-tests/assets/goblins/

rm -rf ../../spine-libgdx/spine-libgdx-tests/assets/raptor/*
cp -f ../raptor/export/*.json ../../spine-libgdx/spine-libgdx-tests/assets/raptor/
cp -f ../raptor/export/*.skel ../../spine-libgdx/spine-libgdx-tests/assets/raptor/
cp -f ../raptor/export/*-pma.* ../../spine-libgdx/spine-libgdx-tests/assets/raptor/

rm -rf ../../spine-libgdx/spine-libgdx-tests/assets/spineboy/*
cp -f ../spineboy/export/*.json ../../spine-libgdx/spine-libgdx-tests/assets/spineboy/
cp -r ../spineboy/export/*.skel ../../spine-libgdx/spine-libgdx-tests/assets/spineboy/
cp -r ../spineboy/export/*-pma.* ../../spine-libgdx/spine-libgdx-tests/assets/spineboy/

echo "spine-as3"
rm -f ../../spine-as3/spine-as3-example/src/spineboy.*
cp -f ../spineboy/export/spineboy-ess.json ../../spine-as3/spine-as3-example/src/
cp -f ../spineboy/export/spineboy.atlas ../../spine-as3/spine-as3-example/src/
cp -f ../spineboy/export/spineboy.png ../../spine-as3/spine-as3-example/src/

echo "spine-cocos2d-objc"
rm -f ../../spine-cocos2d-objc/Resources/*

cp -f ../coin/export/coin-pro.json ../../spine-cocos2d-objc/Resources/
cp -f ../coin/export/coin.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../coin/export/coin.png ../../spine-cocos2d-objc/Resources/

cp -f ../goblins/export/goblins-pro.json ../../spine-cocos2d-objc/Resources/
cp -f ../goblins/export/goblins.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../goblins/export/goblins.png ../../spine-cocos2d-objc/Resources/

cp -f ../raptor/export/raptor-pro.json ../../spine-cocos2d-objc/Resources/
cp -f ../raptor/export/raptor.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../raptor/export/raptor.png ../../spine-cocos2d-objc/Resources/

cp -f ../spineboy/export/spineboy-ess.json ../../spine-cocos2d-objc/Resources/
cp -f ../spineboy/export/spineboy.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../spineboy/export/spineboy.png ../../spine-cocos2d-objc/Resources/

cp -f ../tank/export/tank-pro.json ../../spine-cocos2d-objc/Resources/
cp -f ../tank/export/tank.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../tank/export/tank.png ../../spine-cocos2d-objc/Resources/

echo "spine-cocos2dx"
rm -f ../../spine-cocos2dx/example/Resources/common/*

cp -f ../coin/export/coin-pro.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../coin/export/coin.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../coin/export/coin.png ../../spine-cocos2dx/example/Resources/common/

cp -f ../goblins/export/goblins-pro.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../goblins/export/goblins.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../goblins/export/goblins.png ../../spine-cocos2dx/example/Resources/common/

cp -f ../raptor/export/raptor-pro.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../raptor/export/raptor.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../raptor/export/raptor.png ../../spine-cocos2dx/example/Resources/common/

cp -f ../spineboy/export/spineboy-ess.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../spineboy/export/spineboy.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../spineboy/export/spineboy.png ../../spine-cocos2dx/example/Resources/common/

cp -f ../tank/export/tank-pro.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../tank/export/tank.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../tank/export/tank.png ../../spine-cocos2dx/example/Resources/common/

echo "spine-corona"
rm -f ../../spine-corona/data/*
cp -f ../coin/export/coin-pro.json ../../spine-corona/data
cp -f ../coin/export/coin.atlas ../../spine-corona/data
cp -f ../coin/export/coin.png ../../spine-corona/data

cp -f ../goblins/export/goblins-pro.json ../../spine-corona/data
cp -f ../goblins/export/goblins.atlas ../../spine-corona/data
cp -f ../goblins/export/goblins.png ../../spine-corona/data

cp -f ../raptor/export/raptor-pro.json ../../spine-corona/data
cp -f ../raptor/export/raptor.atlas ../../spine-corona/data
cp -f ../raptor/export/raptor.png ../../spine-corona/data

cp -f ../spineboy/export/spineboy-ess.json ../../spine-corona/data
cp -f ../spineboy/export/spineboy.atlas ../../spine-corona/data
cp -f ../spineboy/export/spineboy.png ../../spine-corona/data

cp -f ../tank/export/tank-pro.json ../../spine-corona/data
cp -f ../tank/export/tank.atlas ../../spine-corona/data
cp -f ../tank/export/tank.png ../../spine-corona/data

cp -f ../vine/export/vine-pro.json ../../spine-corona/data
cp -f ../vine/export/vine.atlas ../../spine-corona/data
cp -f ../vine/export/vine.png ../../spine-corona/data

cp -f ../stretchyman/export/stretchyman-pro.json ../../spine-corona/data
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-corona/data
cp -f ../stretchyman/export/stretchyman.png ../../spine-corona/data

echo "spine-love"
rm -f ../../spine-love/data/*
cp -f ../coin/export/coin-pro.json ../../spine-love/data
cp -f ../coin/export/coin.atlas ../../spine-love/data
cp -f ../coin/export/coin.png ../../spine-love/data

cp -f ../goblins/export/goblins-pro.json ../../spine-love/data
cp -f ../goblins/export/goblins.atlas ../../spine-love/data
cp -f ../goblins/export/goblins.png ../../spine-love/data

cp -f ../raptor/export/raptor-pro.json ../../spine-love/data
cp -f ../raptor/export/raptor.atlas ../../spine-love/data
cp -f ../raptor/export/raptor.png ../../spine-love/data

cp -f ../spineboy/export/spineboy-ess.json ../../spine-love/data
cp -f ../spineboy/export/spineboy.atlas ../../spine-love/data
cp -f ../spineboy/export/spineboy.png ../../spine-love/data

cp -f ../tank/export/tank-pro.json ../../spine-love/data
cp -f ../tank/export/tank.atlas ../../spine-love/data
cp -f ../tank/export/tank.png ../../spine-love/data

cp -f ../vine/export/vine-pro.json ../../spine-love/data
cp -f ../vine/export/vine.atlas ../../spine-love/data
cp -f ../vine/export/vine.png ../../spine-love/data

cp -f ../stretchyman/export/stretchyman-pro.json ../../spine-love/data
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-love/data
cp -f ../stretchyman/export/stretchyman.png ../../spine-love/data

echo "spine-sfml"
rm -f ../../spine-sfml/data/*
cp -f ../coin/export/coin-pro.json ../../spine-sfml/data/
cp -f ../coin/export/coin-pro.skel ../../spine-sfml/data/
cp -f ../coin/export/coin.atlas ../../spine-sfml/data/
cp -f ../coin/export/coin.png ../../spine-sfml/data/

cp -f ../goblins/export/goblins-pro.json ../../spine-sfml/data/
cp -f ../goblins/export/goblins-pro.skel ../../spine-sfml/data/
cp -f ../goblins/export/goblins.atlas ../../spine-sfml/data/
cp -f ../goblins/export/goblins.png ../../spine-sfml/data/

cp -f ../raptor/export/raptor-pro.json ../../spine-sfml/data/
cp -f ../raptor/export/raptor-pro.skel ../../spine-sfml/data/
cp -f ../raptor/export/raptor.atlas ../../spine-sfml/data/
cp -f ../raptor/export/raptor.png ../../spine-sfml/data/

cp -f ../spineboy/export/spineboy-ess.json ../../spine-sfml/data/
cp -f ../spineboy/export/spineboy-ess.skel ../../spine-sfml/data/
cp -f ../spineboy/export/spineboy.atlas ../../spine-sfml/data/
cp -f ../spineboy/export/spineboy.png ../../spine-sfml/data/

cp -f ../tank/export/tank-pro.json ../../spine-sfml/data/
cp -f ../tank/export/tank-pro.skel ../../spine-sfml/data/
cp -f ../tank/export/tank.atlas ../../spine-sfml/data/
cp -f ../tank/export/tank.png ../../spine-sfml/data/

cp -f ../vine/export/vine-pro.json ../../spine-sfml/data/
cp -f ../vine/export/vine-pro.skel ../../spine-sfml/data/
cp -f ../vine/export/vine.atlas ../../spine-sfml/data/
cp -f ../vine/export/vine.png ../../spine-sfml/data/

cp -f ../stretchyman/export/stretchyman-pro.json ../../spine-sfml/data/
cp -f ../stretchyman/export/stretchyman-pro.skel ../../spine-sfml/data/
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-sfml/data/
cp -f ../stretchyman/export/stretchyman.png ../../spine-sfml/data/

echo "spine-starling"
# DO NOT DELETE EVERYTHING IN SOURCE, ESPECIALLY goblins-mesh-starling.png/.xml
cp -f ../coin/export/coin-pro.json ../../spine-starling/spine-starling-example/src/
cp -f ../coin/export/coin.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../coin/export/coin.png ../../spine-starling/spine-starling-example/src/

cp -f ../goblins/export/goblins-pro.json ../../spine-starling/spine-starling-example/src/
cp -f ../goblins/export/goblins.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../goblins/export/goblins.png ../../spine-starling/spine-starling-example/src/

cp -f ../raptor/export/raptor-pro.json ../../spine-starling/spine-starling-example/src/
cp -f ../raptor/export/raptor.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../raptor/export/raptor.png ../../spine-starling/spine-starling-example/src/

cp -f ../spineboy/export/spineboy-ess.json ../../spine-starling/spine-starling-example/src/
cp -f ../spineboy/export/spineboy.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../spineboy/export/spineboy.png ../../spine-starling/spine-starling-example/src/

cp -f ../tank/export/tank-pro.json ../../spine-starling/spine-starling-example/src/
cp -f ../tank/export/tank.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../tank/export/tank.png ../../spine-starling/spine-starling-example/src/

cp -f ../vine/export/vine-pro.json ../../spine-starling/spine-starling-example/src/
cp -f ../vine/export/vine.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../vine/export/vine.png ../../spine-starling/spine-starling-example/src/

cp -f ../stretchyman/export/stretchyman-pro.json ../../spine-starling/spine-starling-example/src/
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../stretchyman/export/stretchyman.png ../../spine-starling/spine-starling-example/src/


echo "spine-ts"
rm -f ../../spine-ts/webgl/example/assets/*
cp -f ../coin/export/coin-pro.json ../../spine-ts/webgl/example/assets/
cp -f ../coin/export/coin.atlas ../../spine-ts/webgl/example/assets/
cp -f ../coin/export/coin.png ../../spine-ts/webgl/example/assets/

cp -f ../goblins/export/goblins-pro.json ../../spine-ts/webgl/example/assets/
cp -f ../goblins/export/goblins.atlas ../../spine-ts/webgl/example/assets/goblins.atlas
cp -f ../goblins/export/goblins.png ../../spine-ts/webgl/example/assets/goblins.png

cp -f ../raptor/export/raptor-pro.json ../../spine-ts/webgl/example/assets/
cp -f ../raptor/export/raptor.atlas ../../spine-ts/webgl/example/assets/
cp -f ../raptor/export/raptor.png ../../spine-ts/webgl/example/assets/

cp -f ../spineboy/export/spineboy-ess.json ../../spine-ts/webgl/example/assets/
cp -f ../spineboy/export/spineboy.atlas ../../spine-ts/webgl/example/assets/
cp -f ../spineboy/export/spineboy.png ../../spine-ts/webgl/example/assets/

cp -f ../tank/export/tank-pro.json ../../spine-ts/webgl/example/assets/
cp -f ../tank/export/tank.atlas ../../spine-ts/webgl/example/assets/
cp -f ../tank/export/tank.png ../../spine-ts/webgl/example/assets/

cp -f ../vine/export/vine-pro.json ../../spine-ts/webgl/example/assets/
cp -f ../vine/export/vine.atlas ../../spine-ts/webgl/example/assets/
cp -f ../vine/export/vine.png ../../spine-ts/webgl/example/assets/

cp -f ../stretchyman/export/stretchyman-pro.json ../../spine-ts/webgl/example/assets/
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-ts/webgl/example/assets/
cp -f ../stretchyman/export/stretchyman.png ../../spine-ts/webgl/example/assets/

rm -f ../../spine-ts/canvas/example/assets/*
cp -f ../spineboy/export/spineboy-ess.json ../../spine-ts/canvas/example/assets/
cp -f ../spineboy/export/spineboy.atlas ../../spine-ts/canvas/example/assets/
cp -f ../spineboy/export/spineboy.png ../../spine-ts/canvas/example/assets/

rm -f ../../spine-ts/threejs/example/assets/*
cp -f ../raptor/export/raptor-pro.json ../../spine-ts/threejs/example/assets/
cp -f ../raptor/export/raptor.atlas ../../spine-ts/threejs/example/assets/
cp -f ../raptor/export/raptor.png ../../spine-ts/threejs/example/assets/

rm -f ../../spine-ts/widget/example/assets/*
cp -f ../raptor/export/raptor-pro.json ../../spine-ts/widget/example/assets/
cp -f ../raptor/export/raptor.atlas ../../spine-ts/widget/example/assets/
cp -f ../raptor/export/raptor.png ../../spine-ts/widget/example/assets/

cp -f ../spineboy/export/spineboy-ess.json ../../spine-ts/widget/example/assets/
cp -f ../spineboy/export/spineboy.atlas ../../spine-ts/widget/example/assets/
cp -f ../spineboy/export/spineboy.png ../../spine-ts/widget/example/assets/

echo "spine-xna"
rm -f ../../spine-xna/example/data/*
cp -f ../coin/export/coin-pro.json ../../spine-xna/example/data/
cp -f ../coin/export/coin-pro.skel ../../spine-xna/example/data/
cp -f ../coin/export/coin.atlas ../../spine-xna/example/data/
cp -f ../coin/export/coin.png ../../spine-xna/example/data/

cp -f ../goblins/export/goblins-pro.json ../../spine-xna/example/data/
cp -f ../goblins/export/goblins-pro.skel ../../spine-xna/example/data/
cp -f ../goblins/export/goblins.atlas ../../spine-xna/example/data/goblins-mesh.atlas
cp -f ../goblins/export/goblins.png ../../spine-xna/example/data/

cp -f ../raptor/export/raptor-pro.json ../../spine-xna/example/data/
cp -f ../raptor/export/raptor-pro.skel ../../spine-xna/example/data/
cp -f ../raptor/export/raptor.atlas ../../spine-xna/example/data/
cp -f ../raptor/export/raptor.png ../../spine-xna/example/data/

cp -f ../spineboy/export/spineboy-ess.json ../../spine-xna/example/data/
cp -f ../spineboy/export/spineboy-ess.skel ../../spine-xna/example/data/
cp -f ../spineboy/export/spineboy.atlas ../../spine-xna/example/data/
cp -f ../spineboy/export/spineboy.png ../../spine-xna/example/data/

cp -f ../tank/export/tank-pro.json ../../spine-xna/example/data/
cp -f ../tank/export/tank-pro.skel ../../spine-xna/example/data/
cp -f ../tank/export/tank.atlas ../../spine-xna/example/data/
cp -f ../tank/export/tank.png ../../spine-xna/example/data/