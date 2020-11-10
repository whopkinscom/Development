@echo off
REM Configuration: %1
REM Project Directory: %2
REM Project Name: %3

set conf=%1
set conf=%conf:"=%
set projdir=%2
set projdir=%projdir:"=%
set projname=%3
set projname=%projname:"=%

IF NOT "%conf%" == "Release" GOTO FINISH
	Echo Nugetting %projname%........
	del "%projdir%\..\packages\%projname%\*.nupkg"
	dotnet pack --no-build --configuration %conf% -o "%projdir%\..\packages\%projname%" /p:PackageVersion=3.2020.1106.15070
REM	del "\\ws-urus\allpayNuGetPackages\Moonrise\%projname%*.nupkg"
REM	echo d | xcopy /f /y "%projdir%\..\packages\%projname%\*.nupkg" "\\ws-urus\allpayNuGetPackages\Moonrise\"
:FINISH

