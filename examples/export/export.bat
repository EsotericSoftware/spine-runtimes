@echo off

echo Cleaning...
del /q ..\alien\export\*
del /q ..\dragon\export\*
del /q ..\goblins\export\*
del /q ..\hero\export\*
del /q ..\powerup\export\*
del /q ..\speedy\export\*
del /q ..\spineboy\export\*
del /q ..\spineboy-old\export\*
del /q ..\spinosaurus\export\*
del /q ..\stretchyman\export\*
del /q ..\raptor\export\*
del /q ..\tank\export\*
del /q ..\vine\export\*

echo.
echo Exporting...
"C:\Program Files (x86)\Spine\Spine.com" ^
-i ../alien/alien.spine -o ../alien/export -e json.json ^
-i ../alien/alien.spine -o ../alien/export -e binary.json ^
-i ../alien/images -o ../alien/export -n alien -p atlas-0.5.json ^
-i ../alien/images -o ../alien/export -n alien-pma -p atlas-0.5-pma.json ^

-i ../dragon/dragon.spine -o ../dragon/export -e json.json ^
-i ../dragon/dragon.spine -o ../dragon/export -e binary.json ^
-i ../dragon/images -o ../dragon/export -n dragon -p atlas-1.0.json ^
-i ../dragon/images -o ../dragon/export -n dragon-pma -p atlas-1.0-pma.json ^

-i ../goblins/goblins.spine -o ../goblins/export -e json.json ^
-i ../goblins/goblins.spine -o ../goblins/export -e binary.json ^
-i ../goblins/goblins-mesh.spine -o ../goblins/export -e json.json ^
-i ../goblins/goblins-mesh.spine -o ../goblins/export -e binary.json ^
-i ../goblins/images -o ../goblins/export -n goblins -p atlas-1.0.json ^
-i ../goblins/images -o ../goblins/export -n goblins-pma -p atlas-1.0-pma.json ^

-i ../hero/hero.spine -o ../hero/export -e json.json ^
-i ../hero/hero.spine -o ../hero/export -e binary.json ^
-i ../hero/hero-mesh.spine -o ../hero/export -e json.json ^
-i ../hero/hero-mesh.spine -o ../hero/export -e binary.json ^
-i ../hero/images -o ../hero/export -n hero -p atlas-1.0.json ^
-i ../hero/images -o ../hero/export -n hero-pma -p atlas-1.0-pma.json ^

-i ../powerup/powerup.spine -o ../powerup/export -e json.json ^
-i ../powerup/powerup.spine -o ../powerup/export -e binary.json ^
-i ../powerup/images -o ../powerup/export -n powerup -p atlas-1.0.json ^
-i ../powerup/images -o ../powerup/export -n powerup-pma -p atlas-1.0-pma.json ^

-i ../speedy/speedy.spine -o ../speedy/export -e json.json ^
-i ../speedy/speedy.spine -o ../speedy/export -e binary.json ^
-i ../speedy/images -o ../speedy/export -n speedy -p atlas-1.0.json ^
-i ../speedy/images -o ../speedy/export -n speedy-pma -p atlas-1.0-pma.json ^

-i ../spineboy/spineboy.spine -o ../spineboy/export -e json.json ^
-i ../spineboy/spineboy.spine -o ../spineboy/export -e binary.json ^
-i ../spineboy/spineboy-mesh.spine -o ../spineboy/export -e json.json ^
-i ../spineboy/spineboy-mesh.spine -o ../spineboy/export -e binary.json ^
-i ../spineboy/images -o ../spineboy/export -n spineboy -p atlas-1.0.json ^
-i ../spineboy/images -o ../spineboy/export -n spineboy-pma -p atlas-1.0-pma.json ^

-i ../spineboy-old/spineboy-old.spine -o ../spineboy-old/export -e json.json ^
-i ../spineboy-old/spineboy-old.spine -o ../spineboy-old/export -e binary.json ^
-i ../spineboy-old/images -o ../spineboy-old/export -n spineboy-old -p atlas-1.0.json ^
-i ../spineboy-old/images -o ../spineboy-old/export -n spineboy-old-pma -p atlas-1.0-pma.json ^
-i ../spineboy-old/normal -o ../spineboy-old/export -n spineboy-old-normal -p atlas-1.0.json ^
-i ../spineboy-old/diffuse -o ../spineboy-old/export -n spineboy-old-diffuse -p atlas-1.0.json ^

-i ../spinosaurus/spinosaurus.spine -o ../spinosaurus/export -e json.json ^
-i ../spinosaurus/spinosaurus.spine -o ../spinosaurus/export -e binary.json ^

-i ../stretchyman/stretchyman.spine -o ../stretchyman/export -e json.json ^
-i ../stretchyman/stretchyman.spine -o ../stretchyman/export -e binary.json ^
-i ../stretchyman/images -o ../stretchyman/export -n stretchyman -p atlas-1.0.json ^
-i ../stretchyman/images -o ../stretchyman/export -n stretchyman-pma -p atlas-1.0-pma.json ^

-i ../raptor/raptor.spine -o ../raptor/export -e json.json ^
-i ../raptor/raptor.spine -o ../raptor/export -e binary.json ^
-i ../raptor/images -o ../raptor/export -n raptor -p atlas-0.5.json ^
-i ../raptor/images -o ../raptor/export -n raptor-pma -p atlas-0.5-pma.json ^

-i ../tank/tank.spine -o ../tank/export -e json.json ^
-i ../tank/tank.spine -o ../tank/export -e binary.json ^
-i ../tank/images -o ../tank/export -n tank -p atlas-0.5.json ^
-i ../tank/images -o ../tank/export -n tank-pma -p atlas-0.5-pma.json ^

-i ../vine/vine.spine -o ../vine/export -e json.json ^
-i ../vine/vine.spine -o ../vine/export -e binary.json ^
-i ../vine/images -o ../vine/export -n vine -p atlas-1.0.json ^
-i ../vine/images -o ../vine/export -n vine-pma -p atlas-1.0-pma.json

del /q ..\spineboy-old\export\*-normal.atlas
