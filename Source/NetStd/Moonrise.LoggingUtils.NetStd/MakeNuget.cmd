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
	dotnet pack --no-build --configuration %conf% -o "%projdir%\..\..\..\GeneratedPackages\%projname%"  /p:NuspecFile="%projname%.nuspec" /p:PackageVersion=4.2020.1217.12261
	nuget sign "%projdir%\..\..\..\GeneratedPackages\%projname%\%projname%.4.2020.1217.12261.nupkg" -Timestamper http://sha256timestamp.ws.symantec.com/sha256/timestamp -CertificatePath "C:\Users\Will\Documents\MMCS.pfx" -CertificatePassword  Password
:FINISH

