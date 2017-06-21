#!/bin/sh
set -e

SPINE_EXE="C:/Program Files (x86)/Spine/Spine.com"
PLATFORM=`uname`
echo $PLATFORM
if [[ $PLATFORM == "Darwin" ]]; then
	SPINE_EXE="/Applications/Spine/Spine.app/Contents/MacOS/Spine"
fi
echo "Spine exe: $SPINE_EXE"

echo "Cleaning..."
rm -rf ../alien/export/*
rm -rf ../coin/export/*
rm -rf ../dragon/export/*
rm -rf ../goblins/export/*
rm -rf ../hero/export/*
rm -rf ../powerup/export/*
rm -rf ../speedy/export/*
rm -rf ../spineboy/export/*
rm -rf ../spinosaurus/export/*
rm -rf ../stretchyman/export/*
rm -rf ../raptor/export/*
rm -rf ../tank/export/*
rm -rf ../vine/export/*

echo ""
echo "Exporting..."
"$SPINE_EXE" \
-i ../alien/alien-ess.spine -o ../alien/export -e json.json \
-i ../alien/alien-ess.spine -o ../alien/export -e binary.json \
-i ../alien/alien-pro.spine -o ../alien/export -e json.json \
-i ../alien/alien-pro.spine -o ../alien/export -e binary.json \
-i ../alien/images -o ../alien/export -n alien -p atlas-0.5.json \
-i ../alien/images -o ../alien/export -n alien-pma -p atlas-0.5-pma.json \
\
-i ../coin/coin-pro.spine -o ../coin/export -e json.json \
-i ../coin/coin-pro.spine -o ../coin/export -e binary.json \
-i ../coin/images -o ../coin/export -n coin -p atlas-1.0.json \
-i ../coin/images -o ../coin/export -n coin-pma -p atlas-1.0-pma.json \
\
-i ../dragon/dragon-ess.spine -o ../dragon/export -e json.json \
-i ../dragon/dragon-ess.spine -o ../dragon/export -e binary.json \
-i ../dragon/images -o ../dragon/export -n dragon -p atlas-1.0.json \
-i ../dragon/images -o ../dragon/export -n dragon-pma -p atlas-1.0-pma.json \
\
-i ../goblins/goblins-ess.spine -o ../goblins/export -e json.json \
-i ../goblins/goblins-ess.spine -o ../goblins/export -e binary.json \
-i ../goblins/goblins-pro.spine -o ../goblins/export -e json.json \
-i ../goblins/goblins-pro.spine -o ../goblins/export -e binary.json \
-i ../goblins/images -o ../goblins/export -n goblins -p atlas-1.0.json \
-i ../goblins/images -o ../goblins/export -n goblins-pma -p atlas-1.0-pma.json \
\
-i ../hero/hero-ess.spine -o ../hero/export -e json.json \
-i ../hero/hero-ess.spine -o ../hero/export -e binary.json \
-i ../hero/hero-pro.spine -o ../hero/export -e json.json \
-i ../hero/hero-pro.spine -o ../hero/export -e binary.json \
-i ../hero/images -o ../hero/export -n hero -p atlas-1.0.json \
-i ../hero/images -o ../hero/export -n hero-pma -p atlas-1.0-pma.json \
\
-i ../powerup/powerup-ess.spine -o ../powerup/export -e json.json \
-i ../powerup/powerup-ess.spine -o ../powerup/export -e binary.json \
-i ../powerup/powerup-pro.spine -o ../powerup/export -e json.json \
-i ../powerup/powerup-pro.spine -o ../powerup/export -e binary.json \
-i ../powerup/images -o ../powerup/export -n powerup -p atlas-1.0.json \
-i ../powerup/images -o ../powerup/export -n powerup-pma -p atlas-1.0-pma.json \
\
-i ../raptor/raptor-pro.spine -o ../raptor/export -e json.json \
-i ../raptor/raptor-pro.spine -o ../raptor/export -e binary.json \
-i ../raptor/images -o ../raptor/export -n raptor -p atlas-0.5.json \
-i ../raptor/images -o ../raptor/export -n raptor-pma -p atlas-0.5-pma.json \
\
-i ../speedy/speedy-ess.spine -o ../speedy/export -e json.json \
-i ../speedy/speedy-ess.spine -o ../speedy/export -e binary.json \
-i ../speedy/images -o ../speedy/export -n speedy -p atlas-1.0.json \
-i ../speedy/images -o ../speedy/export -n speedy-pma -p atlas-1.0-pma.json \
\
-i ../spineboy/spineboy-ess.spine -o ../spineboy/export -e json.json \
-i ../spineboy/spineboy-ess.spine -o ../spineboy/export -e binary.json \
-i ../spineboy/spineboy-pro.spine -o ../spineboy/export -e json.json \
-i ../spineboy/spineboy-pro.spine -o ../spineboy/export -e binary.json \
-i ../spineboy/images -o ../spineboy/export -n spineboy -p atlas-0.5.json \
-i ../spineboy/images -o ../spineboy/export -n spineboy-pma -p atlas-0.5-pma.json \
\
-i ../spinosaurus/spinosaurus-ess.spine -o ../spinosaurus/export -e json.json \
-i ../spinosaurus/spinosaurus-ess.spine -o ../spinosaurus/export -e binary.json \
\
-i ../stretchyman/stretchyman-pro.spine -o ../stretchyman/export -e json.json \
-i ../stretchyman/stretchyman-pro.spine -o ../stretchyman/export -e binary.json \
-i ../stretchyman/images -o ../stretchyman/export -n stretchyman -p atlas-1.0.json \
-i ../stretchyman/images -o ../stretchyman/export -n stretchyman-pma -p atlas-1.0-pma.json \
\
-i ../tank/tank-pro.spine -o ../tank/export -e json.json \
-i ../tank/tank-pro.spine -o ../tank/export -e binary.json \
-i ../tank/images -o ../tank/export -n tank -p atlas-0.5.json \
-i ../tank/images -o ../tank/export -n tank-pma -p atlas-0.5-pma.json \
\
-i ../vine/vine-pro.spine -o ../vine/export -e json.json \
-i ../vine/vine-pro.spine -o ../vine/export -e binary.json \
-i ../vine/images -o ../vine/export -n vine -p atlas-1.0.json \
-i ../vine/images -o ../vine/export -n vine-pma -p atlas-1.0-pma.json
