@echo off
echo Copying to runtimes...

echo.
echo. spine-libgdx
del /q ..\..\spine-libgdx\spine-libgdx-tests\assets\goblins\*
copy /Y ..\goblins\export\*.json ..\..\spine-libgdx\spine-libgdx-tests\assets\goblins\
copy /Y ..\goblins\export\*.skel ..\..\spine-libgdx\spine-libgdx-tests\assets\goblins\
copy /Y ..\goblins\export\*-pma.* ..\..\spine-libgdx\spine-libgdx-tests\assets\goblins\

del /q ..\..\spine-libgdx\spine-libgdx-tests\assets\raptor\*
copy /Y ..\raptor\export\*.json ..\..\spine-libgdx\spine-libgdx-tests\assets\raptor\
copy /Y ..\raptor\export\*.skel ..\..\spine-libgdx\spine-libgdx-tests\assets\raptor\
copy /Y ..\raptor\export\*-pma.* ..\..\spine-libgdx\spine-libgdx-tests\assets\raptor\

del /q ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy\*
copy /Y ..\spineboy\export\*.json ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy\
copy /Y ..\spineboy\export\*.skel ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy\
copy /Y ..\spineboy\export\*-pma.* ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy\

del /q ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy-old\*
copy /Y ..\spineboy-old\export\*.json ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy-old\
copy /Y ..\spineboy-old\export\*.skel ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy-old\
copy /Y ..\spineboy-old\export\*-pma.* ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy-old\
copy /Y ..\spineboy-old\export\*-diffuse.* ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy-old\
copy /Y ..\spineboy-old\export\*-normal.* ..\..\spine-libgdx\spine-libgdx-tests\assets\spineboy-old\
