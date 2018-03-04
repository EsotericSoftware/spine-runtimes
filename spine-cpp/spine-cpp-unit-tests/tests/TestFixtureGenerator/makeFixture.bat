@echo off
if "%1"=="" goto blank

copy "_TestFixture.cpp" "%1TestFixture.cpp"
copy "_TestFixture.h" "%1TestFixture.h"


fnr --cl --find "[[FIXTURE_TYPE]]" --replace "%1TestFixture" --fileMask "%1TestFixture.cpp" --dir %cd%
fnr --cl --find "[[FIXTURE_TYPE]]" --replace "%1TestFixture" --fileMask "%1TestFixture.h" --dir %cd%

goto done

:blank
echo Usage: 
echo       %~n0 FixtureTypeName

:done