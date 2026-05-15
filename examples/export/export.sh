#!/bin/sh

# Usage: export.sh <version> <one or more parameters to pass to Spine, eg: -f --trace>

set -e
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
cd "$SCRIPT_DIR"

SPINE_EXE="C:/Dev/spine/other/installs/Spine/Spine.com"
if [ ! -f "$SPINE_EXE" ]; then
   SPINE_EXE="/mnt/c/Program Files/Spine/Spine.com"
fi
if [ ! -f "$SPINE_EXE" ]; then
   SPINE_EXE="C:/Program Files/Spine/Spine.com"
fi
if [ ! -f "$SPINE_EXE" ]; then
	SPINE_EXE="/Applications/Spine.app/Contents/MacOS/Spine"
fi
echo "Spine exe: $SPINE_EXE"

if [ "$#" -eq 0 ]; then
	echo "Enter the Spine editor version to use for the export (eg 4.3):"
	read version
else
	version=${1%/}
fi

echo "Cleaning export directories..."
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
rm -rf ../owl/export/*
rm -rf ../mix-and-match/export/*
rm -rf ../chibi-stickers/export/*
rm -rf ../celestial-circus/export/*
rm -rf ../snowglobe/export/*
rm -rf ../cloud-pot/export/*
rm -rf ../diamond/export/*
rm -rf ../food-app/export/*
rm -rf ../1-weight-and-mass/export/*
rm -rf ../2-the-12-principles/export/*
rm -rf ../3-timing-and-spacing/export/*
rm -rf ../4-wave-principle/export/*
rm -rf ../5-squash-and-stretch/export/*
rm -rf ../6-arcs/export/*
rm -rf ../7-anticipation/export/*
rm -rf ../8-follow-through/export/*

echo ""
echo "Exporting assets..."
"$SPINE_EXE" \
-u $version ${@:2} --clean-examples \
-i ../alien/alien-ess.spine -o ../alien/export -e json.json \
-i ../alien/alien-ess.spine -o ../alien/export -e binary.json \
-i ../alien/alien-pro.spine -o ../alien/export -e json.json \
-i ../alien/alien-pro.spine -o ../alien/export -e binary.json \
-i ../alien/images -o ../alien/export -n alien -p atlas-0.5.json \
-i ../alien/images -o ../alien/export -n alien-pma -p atlas-0.5-pma.json \
\
-i ../coin/coin-pro.spine -o ../coin/export -e json.json \
-i ../coin/coin-pro.spine -o ../coin/export -e binary.json \
-i ../coin/images -o ../coin/export -n coin -j ../coin/coin-pro.spine -p atlas-1.0.json \
-i ../coin/images -o ../coin/export -n coin-pma -j ../coin/coin-pro.spine -p atlas-1.0-pma.json \
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
-i ../mix-and-match/mix-and-match-pro.spine -o ../mix-and-match/export -e json.json \
-i ../mix-and-match/mix-and-match-pro.spine -o ../mix-and-match/export -e binary.json \
-i ../mix-and-match/images -o ../mix-and-match/export -n mix-and-match -j ../mix-and-match/mix-and-match-pro.spine -p atlas-0.5.json \
-i ../mix-and-match/images -o ../mix-and-match/export -n mix-and-match-pma -j ../mix-and-match/mix-and-match-pro.spine -p atlas-0.5-pma.json \
\
-i ../chibi-stickers/chibi-stickers-pro.spine -o ../chibi-stickers/export -e json.json \
-i ../chibi-stickers/chibi-stickers-pro.spine -o ../chibi-stickers/export -e binary.json \
-i ../chibi-stickers/images -o ../chibi-stickers/export -n chibi-stickers -j ../chibi-stickers/chibi-stickers-pro.spine -p atlas-0.5.json \
-i ../chibi-stickers/images -o ../chibi-stickers/export -n chibi-stickers-pma -j ../chibi-stickers/chibi-stickers-pro.spine -p atlas-0.5-pma.json \
\
-i ../celestial-circus/celestial-circus-pro.spine -o ../celestial-circus/export -e json.json \
-i ../celestial-circus/celestial-circus-pro.spine -o ../celestial-circus/export -e binary.json \
-i ../celestial-circus/images -o ../celestial-circus/export -n celestial-circus -j ../celestial-circus/celestial-circus-pro.spine -p atlas-0.4.json \
-i ../celestial-circus/images -o ../celestial-circus/export -n celestial-circus-pma -j ../celestial-circus/celestial-circus-pro.spine -p atlas-0.4-pma.json \
\
-i ../snowglobe/snowglobe-pro.spine -o ../snowglobe/export -e json.json \
-i ../snowglobe/snowglobe-pro.spine -o ../snowglobe/export -e binary.json \
-i ../snowglobe/images -o ../snowglobe/export -n snowglobe -j ../snowglobe/snowglobe-pro.spine -p atlas-0.5.json \
-i ../snowglobe/images -o ../snowglobe/export -n snowglobe-pma -j ../snowglobe/snowglobe-pro.spine -p atlas-0.5-pma.json \
\
-i ../cloud-pot/cloud-pot-pro.spine -o ../cloud-pot/export -e json.json \
-i ../cloud-pot/cloud-pot-pro.spine -o ../cloud-pot/export -e binary.json \
-i ../cloud-pot/images -o ../cloud-pot/export -n cloud-pot -j ../cloud-pot/cloud-pot-pro.spine -p atlas-0.5.json \
-i ../cloud-pot/images -o ../cloud-pot/export -n cloud-pot-pma -j ../cloud-pot/cloud-pot-pro.spine -p atlas-0.5-pma.json \
\
-i ../diamond/diamond-pro.spine -o ../diamond/export -e json.json \
-i ../diamond/diamond-pro.spine -o ../diamond/export -e binary.json \
-i ../diamond/images -o ../diamond/export -n diamond -j ../diamond/diamond-pro.spine -p atlas-0.5.json \
-i ../diamond/images -o ../diamond/export -n diamond-pma -j ../diamond/diamond-pro.spine -p atlas-0.5-pma.json \
\
-i ../food-app/food-app-ess.spine -o ../food-app/export -e json.json \
-i ../food-app/food-app-ess.spine -o ../food-app/export -e binary.json \
-i ../food-app/food-app-pro.spine -o ../food-app/export -e json.json \
-i ../food-app/food-app-pro.spine -o ../food-app/export -e binary.json \
-i ../food-app/images -o ../food-app/export -n food-app -p atlas-0.4.json \
-i ../food-app/images -o ../food-app/export -n food-app-pma -p atlas-0.4-pma.json \
\
-i ../owl/owl-pro.spine -o ../owl/export -e json.json \
-i ../owl/owl-pro.spine -o ../owl/export -e binary.json \
-i ../owl/images -o ../owl/export -n owl -j ../owl/owl-pro.spine -p atlas-0.5.json \
-i ../owl/images -o ../owl/export -n owl-pma -j ../owl/owl-pro.spine -p atlas-0.5-pma.json \
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
-i ../raptor/images -o ../raptor/export -n raptor -j ../raptor/raptor-pro.spine -p atlas-0.5.json \
-i ../raptor/images -o ../raptor/export -n raptor-pma -j ../raptor/raptor-pro.spine -p atlas-0.5-pma.json \
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
-i ../spineboy/spineboy-pro.spine -o ../spineboy/export/spineboy-run.atlas -e png-0.5-frame-by-frame.json \
-i ../spineboy/images -o ../spineboy/export -n spineboy -p atlas-0.5.json \
-i ../spineboy/images -o ../spineboy/export -n spineboy-pma -p atlas-0.5-pma.json \
\
-i ../spinosaurus/spinosaurus-ess.spine -o ../spinosaurus/export -e json.json \
-i ../spinosaurus/spinosaurus-ess.spine -o ../spinosaurus/export -e binary.json \
\
-i ../stretchyman/stretchyman-pro.spine -o ../stretchyman/export -e json.json \
-i ../stretchyman/stretchyman-pro.spine -o ../stretchyman/export -e binary.json \
-i ../stretchyman/images -o ../stretchyman/export -n stretchyman -j ../stretchyman/stretchyman-pro.spine -p atlas-1.0.json \
-i ../stretchyman/images -o ../stretchyman/export -n stretchyman-pma -j ../stretchyman/stretchyman-pro.spine -p atlas-1.0-pma.json \
\
-i ../tank/tank-pro.spine -o ../tank/export -e json.json \
-i ../tank/tank-pro.spine -o ../tank/export -e binary.json \
-i ../tank/images -o ../tank/export -n tank -j ../tank/tank-pro.spine -p atlas-0.5.json \
-i ../tank/images -o ../tank/export -n tank-pma -j ../tank/tank-pro.spine -p atlas-0.5-pma.json \
\
-i ../vine/vine-pro.spine -o ../vine/export -e json.json \
-i ../vine/vine-pro.spine -o ../vine/export -e binary.json \
-i ../vine/images -o ../vine/export -n vine -p atlas-1.0.json \
-i ../vine/images -o ../vine/export -n vine-pma -p atlas-1.0-pma.json \
\
-i ../windmill/windmill-ess.spine -o ../windmill/export -e json.json \
-i ../windmill/windmill-ess.spine -o ../windmill/export -e binary.json \
-i ../windmill/images -o ../windmill/export -n windmill -p atlas-0.5.json \
-i ../windmill/images -o ../windmill/export -n windmill-pma -p atlas-0.5-pma.json \
\
-i ../1-weight-and-mass/1-weight-and-mass-ess.spine -o ../1-weight-and-mass/export -e json.json \
-i ../1-weight-and-mass/1-weight-and-mass-ess.spine -o ../1-weight-and-mass/export -e binary.json \
-i ../1-weight-and-mass/images -o ../1-weight-and-mass/export -n 1-weight-and-mass -p atlas-0.5.json \
-i ../1-weight-and-mass/images -o ../1-weight-and-mass/export -n 1-weight-and-mass-pma -p atlas-0.5-pma.json \
\
-i ../2-the-12-principles/2-the-12-principles-ess.spine -o ../2-the-12-principles/export -e json.json \
-i ../2-the-12-principles/2-the-12-principles-ess.spine -o ../2-the-12-principles/export -e binary.json \
-i ../2-the-12-principles/images -o ../2-the-12-principles/export -n 2-the-12-principles -p atlas-0.4.json \
-i ../2-the-12-principles/images -o ../2-the-12-principles/export -n 2-the-12-principles-pma -p atlas-0.5-pma.json \
\
-i ../3-timing-and-spacing/3-timing-and-spacing-ess.spine -o ../3-timing-and-spacing/export -e json.json \
-i ../3-timing-and-spacing/3-timing-and-spacing-ess.spine -o ../3-timing-and-spacing/export -e binary.json \
-i ../3-timing-and-spacing/images -o ../3-timing-and-spacing/export -n 3-timing-and-spacing -p atlas-0.5.json \
-i ../3-timing-and-spacing/images -o ../3-timing-and-spacing/export -n 3-timing-and-spacing-pma -p atlas-0.5-pma.json \
\
-i ../4-wave-principle/4-wave-principle-ess.spine -o ../4-wave-principle/export -e json.json \
-i ../4-wave-principle/4-wave-principle-ess.spine -o ../4-wave-principle/export -e binary.json \
-i ../4-wave-principle/images -o ../4-wave-principle/export -n 4-wave-principle -p atlas-0.5.json \
-i ../4-wave-principle/images -o ../4-wave-principle/export -n 4-wave-principle-pma -p atlas-0.5-pma.json \
\
-i ../5-squash-and-stretch/5-squash-and-stretch-ess.spine -o ../5-squash-and-stretch/export -e json.json \
-i ../5-squash-and-stretch/5-squash-and-stretch-ess.spine -o ../5-squash-and-stretch/export -e binary.json \
-i ../5-squash-and-stretch/images -o ../5-squash-and-stretch/export -n 5-squash-and-stretch -p atlas-0.5.json \
-i ../5-squash-and-stretch/images -o ../5-squash-and-stretch/export -n 5-squash-and-stretch-pma -p atlas-0.5-pma.json \
\
-i ../6-arcs/6-arcs-pro.spine -o ../6-arcs/export -e json.json \
-i ../6-arcs/6-arcs-pro.spine -o ../6-arcs/export -e binary.json \
-i ../6-arcs/images -o ../6-arcs/export -n 6-arcs -j ../6-arcs/6-arcs-pro.spine -p atlas-0.5.json \
-i ../6-arcs/images -o ../6-arcs/export -n 6-arcs-pma -j ../6-arcs/6-arcs-pro.spine -p atlas-0.5-pma.json \
\
-i ../7-anticipation/7-anticipation-pro.spine -o ../7-anticipation/export -e json.json \
-i ../7-anticipation/7-anticipation-pro.spine -o ../7-anticipation/export -e binary.json \
-i ../7-anticipation/images -o ../7-anticipation/export -n 7-anticipation -j ../7-anticipation/7-anticipation-pro.spine -p atlas-0.5.json \
-i ../7-anticipation/images -o ../7-anticipation/export -n 7-anticipation-pma -j ../7-anticipation/7-anticipation-pro.spine -p atlas-0.5-pma.json \
\
-i ../8-follow-through/8-follow-through-pro.spine -o ../8-follow-through/export -e json.json \
-i ../8-follow-through/8-follow-through-pro.spine -o ../8-follow-through/export -e binary.json \
-i ../8-follow-through/images -o ../8-follow-through/export -n 8-follow-through -j ../8-follow-through/8-follow-through-pro.spine -p atlas-0.5.json \
-i ../8-follow-through/images -o ../8-follow-through/export -n 8-follow-through-pma -j ../8-follow-through/8-follow-through-pro.spine -p atlas-0.5-pma.json

# spineboy-old.spine needs separate export, as its images are in an atlas.
"$SPINE_EXE" \
-u $version ${@:2} \
-i ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old/spineboy-old.spine -o ../../spine-libgdx/spine-libgdx-tests/assets/spineboy-old -e json.json

# Export Unity Assets
UNITY_BASE_DIR=../spine-unity
if [ -d "$UNITY_BASE_DIR" ]; then
	echo "Cleaning Unity export directories..."
	rm -rf $UNITY_BASE_DIR/eyes/export/*
	rm -rf $UNITY_BASE_DIR/footsoldier/export/*
	rm -rf $UNITY_BASE_DIR/gauge/export/*
	rm -rf $UNITY_BASE_DIR/raggedyspineboy/export/*
	rm -rf $UNITY_BASE_DIR/raptor/export/*
	rm -rf $UNITY_BASE_DIR/spineboy-pro/export/*
	rm -rf $UNITY_BASE_DIR/spineboy-unity/export/*
	rm -rf $UNITY_BASE_DIR/spineunitygirl/export/*
	rm -rf $UNITY_BASE_DIR/whirlyblendmodes/export/*

	echo ""
	echo "Exporting Unity assets..."
	"$SPINE_EXE" -u $version ${@:2} --clean-examples \
	-i $UNITY_BASE_DIR/eyes/eyes.spine -o $UNITY_BASE_DIR/eyes/export -e json.json \
	-i $UNITY_BASE_DIR/eyes/eyes.spine -o $UNITY_BASE_DIR/eyes/export -e binary.json \
	-i $UNITY_BASE_DIR/eyes/images -o $UNITY_BASE_DIR/eyes/export -n eyes -p atlas-1.0.json \
	-i $UNITY_BASE_DIR/eyes/images -o $UNITY_BASE_DIR/eyes/export -n eyes-pma -p atlas-1.0-pma.json \
	\
	-i $UNITY_BASE_DIR/footsoldier/footsoldier.spine -o $UNITY_BASE_DIR/footsoldier/export -e json.json \
	-i $UNITY_BASE_DIR/footsoldier/footsoldier.spine -o $UNITY_BASE_DIR/footsoldier/export -e binary.json \
	-i $UNITY_BASE_DIR/footsoldier/images -o $UNITY_BASE_DIR/footsoldier/export -n footsoldier -p atlas-1.0.json \
	-i $UNITY_BASE_DIR/footsoldier/images -o $UNITY_BASE_DIR/footsoldier/export -n footsoldier-pma -p atlas-1.0-pma.json \
	\
	-i $UNITY_BASE_DIR/gauge/gauge.spine -o $UNITY_BASE_DIR/gauge/export -e json.json \
	-i $UNITY_BASE_DIR/gauge/gauge.spine -o $UNITY_BASE_DIR/gauge/export -e binary.json \
	-i $UNITY_BASE_DIR/gauge/images -o $UNITY_BASE_DIR/gauge/export -n gauge -p atlas-1.0.json \
	-i $UNITY_BASE_DIR/gauge/images -o $UNITY_BASE_DIR/gauge/export -n gauge-pma -p atlas-1.0-pma.json \
	\
	-i $UNITY_BASE_DIR/raggedyspineboy/raggedyspineboy.spine -o $UNITY_BASE_DIR/raggedyspineboy/export -e json.json \
	-i $UNITY_BASE_DIR/raggedyspineboy/raggedyspineboy.spine -o $UNITY_BASE_DIR/raggedyspineboy/export -e binary.json \
	-i $UNITY_BASE_DIR/raggedyspineboy/images -o $UNITY_BASE_DIR/raggedyspineboy/export -n raggedyspineboy -p atlas-1.0.json \
	-i $UNITY_BASE_DIR/raggedyspineboy/images -o $UNITY_BASE_DIR/raggedyspineboy/export -n raggedyspineboy-pma -p atlas-1.0-pma.json \
	\
	-i $UNITY_BASE_DIR/raptor/raptor.spine -o $UNITY_BASE_DIR/raptor/export -e json.json \
	-i $UNITY_BASE_DIR/raptor/raptor.spine -o $UNITY_BASE_DIR/raptor/export -e binary.json \
	-i $UNITY_BASE_DIR/raptor/images -o $UNITY_BASE_DIR/raptor/export -n raptor -p atlas-1.0-square.json \
	-i $UNITY_BASE_DIR/raptor/images -o $UNITY_BASE_DIR/raptor/export -n raptor-pma -p atlas-1.0-square-pma.json \
	\
	-i $UNITY_BASE_DIR/spineboy-pro/spineboy-pro.spine -o $UNITY_BASE_DIR/spineboy-pro/export -e json.json \
	-i $UNITY_BASE_DIR/spineboy-pro/spineboy-pro.spine -o $UNITY_BASE_DIR/spineboy-pro/export -e binary.json \
	-i $UNITY_BASE_DIR/spineboy-pro/images -o $UNITY_BASE_DIR/spineboy-pro/export -n spineboy -j $UNITY_BASE_DIR/spineboy-pro/spineboy-pro.spine -p atlas-1.0-2048.json \
	-i $UNITY_BASE_DIR/spineboy-pro/images -o $UNITY_BASE_DIR/spineboy-pro/export -n spineboy-pma -j $UNITY_BASE_DIR/spineboy-pro/spineboy-pro.spine -p atlas-1.0-2048-pma.json \
	\
	-i $UNITY_BASE_DIR/spineboy-unity/spineboy-unity.spine -o $UNITY_BASE_DIR/spineboy-unity/export -e json.json \
	-i $UNITY_BASE_DIR/spineboy-unity/spineboy-unity.spine -o $UNITY_BASE_DIR/spineboy-unity/export -e binary.json \
	-i $UNITY_BASE_DIR/spineboy-unity/images -o $UNITY_BASE_DIR/spineboy-unity/export -n spineboy -p atlas-1.0-square.json \
	-i $UNITY_BASE_DIR/spineboy-unity/images -o $UNITY_BASE_DIR/spineboy-unity/export -n spineboy-pma -p atlas-1.0-square-pma.json \
	\
	-i $UNITY_BASE_DIR/spineunitygirl/doi.spine -o $UNITY_BASE_DIR/spineunitygirl/export -e json.json \
	-i $UNITY_BASE_DIR/spineunitygirl/doi.spine -o $UNITY_BASE_DIR/spineunitygirl/export -e binary.json \
	-i $UNITY_BASE_DIR/spineunitygirl/images -o $UNITY_BASE_DIR/spineunitygirl/export -n doi -p atlas-1.0.json \
	-i $UNITY_BASE_DIR/spineunitygirl/images -o $UNITY_BASE_DIR/spineunitygirl/export -n doi-pma -p atlas-1.0-pma.json \
	\
	-i $UNITY_BASE_DIR/whirlyblendmodes/whirlyblendmodes.spine -o $UNITY_BASE_DIR/whirlyblendmodes/export -e json.json \
	-i $UNITY_BASE_DIR/whirlyblendmodes/whirlyblendmodes.spine -o $UNITY_BASE_DIR/whirlyblendmodes/export -e binary.json \
	-i $UNITY_BASE_DIR/whirlyblendmodes/images -o $UNITY_BASE_DIR/whirlyblendmodes/export -n whirlyblendmodes -p atlas-1.0.json \
	-i $UNITY_BASE_DIR/whirlyblendmodes/images -o $UNITY_BASE_DIR/whirlyblendmodes/export -n whirlyblendmodes-pma -p atlas-1.0-pma.json
fi

echo ""
echo "Optimizing PNGs..."
cd ..
find . -iname '*.png' -exec oxipng -q --nx -i 0 -o 4 --strip all {} +

echo ""
echo "Done!"
