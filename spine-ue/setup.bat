@echo off
rmdir Plugins\SpinePlugin\Source\SpinePlugin\Public\spine-cpp /s /q
xcopy /E /I ..\spine-cpp\spine-cpp Plugins\SpinePlugin\Source\SpinePlugin\Public\spine-cpp  || goto error
goto done

:error
@echo Couldn^'t setup spine-ue

:done