#!/bin/sh
set -e
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
cd $SCRIPT_DIR

SPINE_EXE="C:/Program Files (x86)/Spine/Spine.com"
if [ ! -f "$SPINE_EXE" ]; then
   SPINE_EXE="/mnt/c/Program Files (x86)/Spine/Spine.com"
fi
if [ ! -f "$SPINE_EXE" ]; then
	SPINE_EXE="/Applications/Spine/Spine.app/Contents/MacOS/Spine"
fi
echo "Spine exe: $SPINE_EXE"

echo "Please enter the Spine editor version to use to clean the examples (e.g. 3.7.58-beta)"
read version

PROJECTS_BASE_DIR=../spine-unity

echo "Cleaning export directories ..."
rm -rf $PROJECTS_BASE_DIR/eyes/export/*
rm -rf $PROJECTS_BASE_DIR/footsoldier/export/*
rm -rf $PROJECTS_BASE_DIR/gauge/export/*
rm -rf $PROJECTS_BASE_DIR/raggedyspineboy/export/*
rm -rf $PROJECTS_BASE_DIR/spineboy-unity/export/*
rm -rf $PROJECTS_BASE_DIR/spineunitygirl/export/*
rm -rf $PROJECTS_BASE_DIR/whirlyblendmodes/export/*

echo ""
echo "Exporting..."
"$SPINE_EXE" \
-u $version -f \
-i $PROJECTS_BASE_DIR/eyes/eyes.spine -o $PROJECTS_BASE_DIR/eyes/export -e json.json \
-i $PROJECTS_BASE_DIR/eyes/eyes.spine -o $PROJECTS_BASE_DIR/eyes/export -e binary.json \
-i $PROJECTS_BASE_DIR/eyes/images -o $PROJECTS_BASE_DIR/eyes/export -n eyes -p atlas-1.0.json \
-i $PROJECTS_BASE_DIR/eyes/images -o $PROJECTS_BASE_DIR/eyes/export -n eyes-pma -p atlas-1.0-pma.json \
\
-i $PROJECTS_BASE_DIR/footsoldier/footsoldier.spine -o $PROJECTS_BASE_DIR/footsoldier/export -e json.json \
-i $PROJECTS_BASE_DIR/footsoldier/footsoldier.spine -o $PROJECTS_BASE_DIR/footsoldier/export -e binary.json \
-i $PROJECTS_BASE_DIR/footsoldier/images -o $PROJECTS_BASE_DIR/footsoldier/export -n footsoldier -p atlas-1.0.json \
-i $PROJECTS_BASE_DIR/footsoldier/images -o $PROJECTS_BASE_DIR/footsoldier/export -n footsoldier-pma -p atlas-1.0-pma.json \
\
-i $PROJECTS_BASE_DIR/gauge/gauge.spine -o $PROJECTS_BASE_DIR/gauge/export -e json.json \
-i $PROJECTS_BASE_DIR/gauge/gauge.spine -o $PROJECTS_BASE_DIR/gauge/export -e binary.json \
-i $PROJECTS_BASE_DIR/gauge/images -o $PROJECTS_BASE_DIR/gauge/export -n gauge -p atlas-1.0.json \
-i $PROJECTS_BASE_DIR/gauge/images -o $PROJECTS_BASE_DIR/gauge/export -n gauge-pma -p atlas-1.0-pma.json \
\
-i $PROJECTS_BASE_DIR/hero/hero-pro.spine -o $PROJECTS_BASE_DIR/hero/export -e json.json \
-i $PROJECTS_BASE_DIR/hero/hero-pro.spine -o $PROJECTS_BASE_DIR/hero/export -e binary.json \
-i $PROJECTS_BASE_DIR/hero/images -o $PROJECTS_BASE_DIR/hero/export -n hero -p atlas-1.0.json \
-i $PROJECTS_BASE_DIR/hero/images -o $PROJECTS_BASE_DIR/hero/export -n hero-pma -p atlas-1.0-pma.json \
\
-i $PROJECTS_BASE_DIR/raggedyspineboy/raggedyspineboy.spine -o $PROJECTS_BASE_DIR/raggedyspineboy/export -e json.json \
-i $PROJECTS_BASE_DIR/raggedyspineboy/raggedyspineboy.spine -o $PROJECTS_BASE_DIR/raggedyspineboy/export -e binary.json \
-i $PROJECTS_BASE_DIR/raggedyspineboy/images -o $PROJECTS_BASE_DIR/raggedyspineboy/export -n raggedyspineboy -p atlas-1.0.json \
-i $PROJECTS_BASE_DIR/raggedyspineboy/images -o $PROJECTS_BASE_DIR/raggedyspineboy/export -n raggedyspineboy-pma -p atlas-1.0-pma.json \
\
-i $PROJECTS_BASE_DIR/raptor/raptor.spine -o $PROJECTS_BASE_DIR/raptor/export -e json.json \
-i $PROJECTS_BASE_DIR/raptor/raptor.spine -o $PROJECTS_BASE_DIR/raptor/export -e binary.json \
-i $PROJECTS_BASE_DIR/raptor/images -o $PROJECTS_BASE_DIR/raptor/export -n raptor -p atlas-1.0-square.json \
-i $PROJECTS_BASE_DIR/raptor/images -o $PROJECTS_BASE_DIR/raptor/export -n raptor-pma -p atlas-1.0-square-pma.json \
\
-i $PROJECTS_BASE_DIR/spineboy-pro/spineboy-pro.spine -o $PROJECTS_BASE_DIR/spineboy-pro/export -e json.json \
-i $PROJECTS_BASE_DIR/spineboy-pro/spineboy-pro.spine -o $PROJECTS_BASE_DIR/spineboy-pro/export -e binary.json \
-i $PROJECTS_BASE_DIR/spineboy-pro/images -o $PROJECTS_BASE_DIR/spineboy-pro/export -n spineboy -p atlas-1.0-2048.json \
-i $PROJECTS_BASE_DIR/spineboy-pro/images -o $PROJECTS_BASE_DIR/spineboy-pro/export -n spineboy-pma -p atlas-1.0-2048-pma.json \
\
-i $PROJECTS_BASE_DIR/spineboy-unity/spineboy-unity.spine -o $PROJECTS_BASE_DIR/spineboy-unity/export -e json.json \
-i $PROJECTS_BASE_DIR/spineboy-unity/spineboy-unity.spine -o $PROJECTS_BASE_DIR/spineboy-unity/export -e binary.json \
-i $PROJECTS_BASE_DIR/spineboy-unity/images -o $PROJECTS_BASE_DIR/spineboy-unity/export -n spineboy -p atlas-1.0-square.json \
-i $PROJECTS_BASE_DIR/spineboy-unity/images -o $PROJECTS_BASE_DIR/spineboy-unity/export -n spineboy-pma -p atlas-1.0-square-pma.json \
\
-i $PROJECTS_BASE_DIR/spineunitygirl/doi.spine -o $PROJECTS_BASE_DIR/spineunitygirl/export -e json.json \
-i $PROJECTS_BASE_DIR/spineunitygirl/doi.spine -o $PROJECTS_BASE_DIR/spineunitygirl/export -e binary.json \
-i $PROJECTS_BASE_DIR/spineunitygirl/images -o $PROJECTS_BASE_DIR/spineunitygirl/export -n doi -p atlas-1.0.json \
-i $PROJECTS_BASE_DIR/spineunitygirl/images -o $PROJECTS_BASE_DIR/spineunitygirl/export -n doi-pma -p atlas-1.0-pma.json \
\
-i $PROJECTS_BASE_DIR/whirlyblendmodes/whirlyblendmodes.spine -o $PROJECTS_BASE_DIR/whirlyblendmodes/export -e json.json \
-i $PROJECTS_BASE_DIR/whirlyblendmodes/whirlyblendmodes.spine -o $PROJECTS_BASE_DIR/whirlyblendmodes/export -e binary.json \
-i $PROJECTS_BASE_DIR/whirlyblendmodes/images -o $PROJECTS_BASE_DIR/whirlyblendmodes/export -n whirlyblendmodes -p atlas-1.0.json \
-i $PROJECTS_BASE_DIR/whirlyblendmodes/images -o $PROJECTS_BASE_DIR/whirlyblendmodes/export -n whirlyblendmodes-pma -p atlas-1.0-pma.json