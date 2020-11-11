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
	del "%projdir%\..\GeneratedPackages\%projname%\*.nupkg"
	dotnet pack --no-build --configuration %conf% -o "%projdir%\..\GeneratedPackages\%projname%" /p:PackageVersion=4.2020.1110.19400
:FINISH

