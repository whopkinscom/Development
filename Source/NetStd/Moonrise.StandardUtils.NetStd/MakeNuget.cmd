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
	del /Q "%projdir%\..\..\..\GeneratedPackages\%projname%\*.*"
	dotnet pack --no-build --configuration %conf% -o "%projdir%\..\..\..\GeneratedPackages\%projname%" /p:PackageVersion=4.2020.1116.12275
REM	del "\\ws-urus\allpayNuGetPackages\Moonrise\%projname%*.nupkg"
REM	echo d | xcopy /f /y "%projdir%\..\packages\%projname%\*.nupkg" "\\ws-urus\allpayNuGetPackages\Moonrise\"
:FINISH

