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

rm -rf ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old/*
cp -f ../spineboy-old/export/*.json ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old/
cp -f ../spineboy-old/export/*.skel ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old/
cp -f ../spineboy-old/export/*-pma.* ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old/
cp -f ../spineboy-old/export/*-diffuse.* ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old/
cp -f ../spineboy-old/export/*-normal.* ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old/

echo "spine-as3"
rm -f ../../spine-as3/spine-as3-example/src/spineboy.*
cp -f ../spineboy/export/spineboy.json ../../spine-as3/spine-as3-example/src/
cp -f ../spineboy/export/spineboy.atlas ../../spine-as3/spine-as3-example/src/
cp -f ../spineboy/export/spineboy.png ../../spine-as3/spine-as3-example/src/

echo "spine-cocos2d-objc"
rm -f ../../spine-cocos2d-objc/Resources/*
cp -f ../goblins/export/goblins-mesh.json ../../spine-cocos2d-objc/Resources/
cp -f ../goblins/export/goblins.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../goblins/export/goblins.png ../../spine-cocos2d-objc/Resources/

cp -f ../raptor/export/raptor.json ../../spine-cocos2d-objc/Resources/
cp -f ../raptor/export/raptor.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../raptor/export/raptor.png ../../spine-cocos2d-objc/Resources/

cp -f ../spineboy/export/spineboy.json ../../spine-cocos2d-objc/Resources/
cp -f ../spineboy/export/spineboy.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../spineboy/export/spineboy.png ../../spine-cocos2d-objc/Resources/

cp -f ../tank/export/tank.json ../../spine-cocos2d-objc/Resources/
cp -f ../tank/export/tank.atlas ../../spine-cocos2d-objc/Resources/
cp -f ../tank/export/tank.png ../../spine-cocos2d-objc/Resources/

echo "spine-cocos2dx"
rm -f ../../spine-cocos2dx/example/Resources/common/*
cp -f ../goblins/export/goblins-mesh.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../goblins/export/goblins.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../goblins/export/goblins.png ../../spine-cocos2dx/example/Resources/common/

cp -f ../raptor/export/raptor.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../raptor/export/raptor.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../raptor/export/raptor.png ../../spine-cocos2dx/example/Resources/common/

cp -f ../spineboy/export/spineboy.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../spineboy/export/spineboy.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../spineboy/export/spineboy.png ../../spine-cocos2dx/example/Resources/common/

cp -f ../tank/export/tank.json ../../spine-cocos2dx/example/Resources/common/
cp -f ../tank/export/tank.atlas ../../spine-cocos2dx/example/Resources/common/
cp -f ../tank/export/tank.png ../../spine-cocos2dx/example/Resources/common/

echo "spine-corona"
rm -f ../../spine-corona/data/*
cp -f ../goblins/export/goblins-mesh.json ../../spine-corona/data
cp -f ../goblins/export/goblins.atlas ../../spine-corona/data
cp -f ../goblins/export/goblins.png ../../spine-corona/data

cp -f ../raptor/export/raptor.json ../../spine-corona/data
cp -f ../raptor/export/raptor.atlas ../../spine-corona/data
cp -f ../raptor/export/raptor.png ../../spine-corona/data

cp -f ../spineboy/export/spineboy.json ../../spine-corona/data
cp -f ../spineboy/export/spineboy.atlas ../../spine-corona/data
cp -f ../spineboy/export/spineboy.png ../../spine-corona/data

cp -f ../tank/export/tank.json ../../spine-corona/data
cp -f ../tank/export/tank.atlas ../../spine-corona/data
cp -f ../tank/export/tank.png ../../spine-corona/data

cp -f ../vine/export/vine.json ../../spine-corona/data
cp -f ../vine/export/vine.atlas ../../spine-corona/data
cp -f ../vine/export/vine.png ../../spine-corona/data

cp -f ../stretchyman/export/stretchyman.json ../../spine-corona/data
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-corona/data
cp -f ../stretchyman/export/stretchyman.png ../../spine-corona/data

cp -f ../test/export/test.json ../../spine-corona/data
cp -f ../test/export/test.atlas ../../spine-corona/data
cp -f ../test/export/test.png ../../spine-corona/data

echo "spine-love"
rm -f ../../spine-love/data/*
cp -f ../goblins/export/goblins-mesh.json ../../spine-love/data
cp -f ../goblins/export/goblins.atlas ../../spine-love/data
cp -f ../goblins/export/goblins.png ../../spine-love/data

cp -f ../raptor/export/raptor.json ../../spine-love/data
cp -f ../raptor/export/raptor.atlas ../../spine-love/data
cp -f ../raptor/export/raptor.png ../../spine-love/data

cp -f ../spineboy/export/spineboy.json ../../spine-love/data
cp -f ../spineboy/export/spineboy.atlas ../../spine-love/data
cp -f ../spineboy/export/spineboy.png ../../spine-love/data

cp -f ../tank/export/tank.json ../../spine-love/data
cp -f ../tank/export/tank.atlas ../../spine-love/data
cp -f ../tank/export/tank.png ../../spine-love/data

cp -f ../vine/export/vine.json ../../spine-love/data
cp -f ../vine/export/vine.atlas ../../spine-love/data
cp -f ../vine/export/vine.png ../../spine-love/data

cp -f ../stretchyman/export/stretchyman.json ../../spine-love/data
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-love/data
cp -f ../stretchyman/export/stretchyman.png ../../spine-love/data

cp -f ../test/export/test.json ../../spine-love/data
cp -f ../test/export/test.atlas ../../spine-love/data
cp -f ../test/export/test.png ../../spine-love/data

echo "spine-sfml"
rm -f ../../spine-sfml/data/*
cp -f ../goblins/export/goblins-mesh.json ../../spine-sfml/data/
cp -f ../goblins/export/goblins-mesh.skel ../../spine-sfml/data/
cp -f ../goblins/export/goblins.atlas ../../spine-sfml/data/
cp -f ../goblins/export/goblins.png ../../spine-sfml/data/

cp -f ../raptor/export/raptor.json ../../spine-sfml/data/
cp -f ../raptor/export/raptor.skel ../../spine-sfml/data/
cp -f ../raptor/export/raptor.atlas ../../spine-sfml/data/
cp -f ../raptor/export/raptor.png ../../spine-sfml/data/

cp -f ../spineboy/export/spineboy.json ../../spine-sfml/data/
cp -f ../spineboy/export/spineboy.skel ../../spine-sfml/data/
cp -f ../spineboy/export/spineboy.atlas ../../spine-sfml/data/
cp -f ../spineboy/export/spineboy.png ../../spine-sfml/data/

cp -f ../tank/export/tank.json ../../spine-sfml/data/
cp -f ../tank/export/tank.skel ../../spine-sfml/data/
cp -f ../tank/export/tank.atlas ../../spine-sfml/data/
cp -f ../tank/export/tank.png ../../spine-sfml/data/

cp -f ../vine/export/vine.json ../../spine-sfml/data/
cp -f ../vine/export/vine.skel ../../spine-sfml/data/
cp -f ../vine/export/vine.atlas ../../spine-sfml/data/
cp -f ../vine/export/vine.png ../../spine-sfml/data/

cp -f ../stretchyman/export/stretchyman.json ../../spine-sfml/data/
cp -f ../stretchyman/export/stretchyman.skel ../../spine-sfml/data/
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-sfml/data/
cp -f ../stretchyman/export/stretchyman.png ../../spine-sfml/data/

echo "spine-starling"
# DO NOT DELETE EVERYTHING IN SOURCE, ESPECIALLY goblins-mesh-starling.png/.xml
cp -f ../goblins/export/goblins-mesh.json ../../spine-starling/spine-starling-example/src/
cp -f ../goblins/export/goblins.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../goblins/export/goblins.png ../../spine-starling/spine-starling-example/src/

cp -f ../raptor/export/raptor.json ../../spine-starling/spine-starling-example/src/
cp -f ../raptor/export/raptor.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../raptor/export/raptor.png ../../spine-starling/spine-starling-example/src/

cp -f ../spineboy/export/spineboy.json ../../spine-starling/spine-starling-example/src/
cp -f ../spineboy/export/spineboy.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../spineboy/export/spineboy.png ../../spine-starling/spine-starling-example/src/

cp -f ../tank/export/tank.json ../../spine-starling/spine-starling-example/src/
cp -f ../tank/export/tank.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../tank/export/tank.png ../../spine-starling/spine-starling-example/src/

cp -f ../vine/export/vine.json ../../spine-starling/spine-starling-example/src/
cp -f ../vine/export/vine.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../vine/export/vine.png ../../spine-starling/spine-starling-example/src/

cp -f ../stretchyman/export/stretchyman.json ../../spine-starling/spine-starling-example/src/
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-starling/spine-starling-example/src/
cp -f ../stretchyman/export/stretchyman.png ../../spine-starling/spine-starling-example/src/


echo "spine-ts"
rm -f ../../spine-ts/webgl/example/assets/*
cp -f ../goblins/export/goblins-mesh.json ../../spine-ts/webgl/example/assets/
cp -f ../goblins/export/goblins.atlas ../../spine-ts/webgl/example/assets/goblins-mesh.atlas
cp -f ../goblins/export/goblins.png ../../spine-ts/webgl/example/assets/goblins.png

cp -f ../raptor/export/raptor.json ../../spine-ts/webgl/example/assets/
cp -f ../raptor/export/raptor.atlas ../../spine-ts/webgl/example/assets/
cp -f ../raptor/export/raptor.png ../../spine-ts/webgl/example/assets/

cp -f ../spineboy/export/spineboy.json ../../spine-ts/webgl/example/assets/
cp -f ../spineboy/export/spineboy.atlas ../../spine-ts/webgl/example/assets/
cp -f ../spineboy/export/spineboy.png ../../spine-ts/webgl/example/assets/

cp -f ../tank/export/tank.json ../../spine-ts/webgl/example/assets/
cp -f ../tank/export/tank.atlas ../../spine-ts/webgl/example/assets/
cp -f ../tank/export/tank.png ../../spine-ts/webgl/example/assets/

cp -f ../vine/export/vine.json ../../spine-ts/webgl/example/assets/
cp -f ../vine/export/vine.atlas ../../spine-ts/webgl/example/assets/
cp -f ../vine/export/vine.png ../../spine-ts/webgl/example/assets/

cp -f ../stretchyman/export/stretchyman.json ../../spine-ts/webgl/example/assets/
cp -f ../stretchyman/export/stretchyman.atlas ../../spine-ts/webgl/example/assets/
cp -f ../stretchyman/export/stretchyman.png ../../spine-ts/webgl/example/assets/

rm -f ../../spine-ts/canvas/example/assets/*
cp -f ../spineboy/export/spineboy.json ../../spine-ts/canvas/example/assets/
cp -f ../spineboy/export/spineboy.atlas ../../spine-ts/canvas/example/assets/
cp -f ../spineboy/export/spineboy.png ../../spine-ts/canvas/example/assets/

rm -f ../../spine-ts/threejs/example/assets/*
cp -f ../raptor/export/raptor.json ../../spine-ts/threejs/example/assets/
cp -f ../raptor/export/raptor.atlas ../../spine-ts/threejs/example/assets/
cp -f ../raptor/export/raptor.png ../../spine-ts/threejs/example/assets/

rm -f ../../spine-ts/widget/example/assets/*
cp -f ../raptor/export/raptor.json ../../spine-ts/widget/example/assets/
cp -f ../raptor/export/raptor.atlas ../../spine-ts/widget/example/assets/
cp -f ../raptor/export/raptor.png ../../spine-ts/widget/example/assets/

cp -f ../spineboy/export/spineboy.json ../../spine-ts/widget/example/assets/
cp -f ../spineboy/export/spineboy.atlas ../../spine-ts/widget/example/assets/
cp -f ../spineboy/export/spineboy.png ../../spine-ts/widget/example/assets/

echo "spine-xna"
rm -f ../../spine-xna/example/data/*
cp -f ../goblins/export/goblins-mesh.json ../../spine-xna/example/data/
cp -f ../goblins/export/goblins-mesh.skel ../../spine-xna/example/data/
cp -f ../goblins/export/goblins.atlas ../../spine-xna/example/data/goblins-mesh.atlas
cp -f ../goblins/export/goblins.png ../../spine-xna/example/data/

cp -f ../raptor/export/raptor.json ../../spine-xna/example/data/
cp -f ../raptor/export/raptor.skel ../../spine-xna/example/data/
cp -f ../raptor/export/raptor.atlas ../../spine-xna/example/data/
cp -f ../raptor/export/raptor.png ../../spine-xna/example/data/

cp -f ../spineboy/export/spineboy.json ../../spine-xna/example/data/
cp -f ../spineboy/export/spineboy.skel ../../spine-xna/example/data/
cp -f ../spineboy/export/spineboy.atlas ../../spine-xna/example/data/
cp -f ../spineboy/export/spineboy.png ../../spine-xna/example/data/

cp -f ../tank/export/tank.json ../../spine-xna/example/data/
cp -f ../tank/export/tank.skel ../../spine-xna/example/data/
cp -f ../tank/export/tank.atlas ../../spine-xna/example/data/
cp -f ../tank/export/tank.png ../../spine-xna/example/data/
