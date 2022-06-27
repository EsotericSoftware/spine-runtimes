@echo off
if [%1]==[] goto usage

set branch=%1
rmdir godot /s /q
git clone --depth 1 https://github.com/godotengine/godot.git -b %branch%  || goto error
xcopy /E /I .idea godot\.idea  || goto error
copy custom.py godot  || goto error
rmdir spine_godot\spine-cpp /s /q
xcopy /E /I ..\spine-cpp\spine-cpp spine_godot\spine-cpp  || goto error
cd godot & git apply ../livepp.patch & git apply ../livepp-v4.patch & cd ..
build.bat || goto error
exit 0

:usage
@echo.
@echo Usage^: setup.bat ^<godot_branch_or_tag^>
@echo.    
@echo e.g.:
@echo        setup.bat 3.4.4-stable
@echo        setup.bat master
exit 1

:error
@echo Couldn^'t setup Godot