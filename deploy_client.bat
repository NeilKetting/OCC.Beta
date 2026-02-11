@echo off
setlocal

:: Set version - bump this manually or automate as needed
set VERSION=1.6.12

echo ========================================================
echo [CLIENT] RELEASE BUILD AND PACKAGE AUTOMATION (v%VERSION%)
echo ========================================================

:: Ensure we are in the repository root
cd /d "%~dp0"

:: 1. Clean and Build
echo [BUILD] Compiling OCC.Client Desktop (Self-Contained win-x64)...
dotnet publish "OCC.Client\OCC.Client.Desktop\OCC.Client.Desktop.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfContained=true -o publish

if %errorlevel% neq 0 (
    echo [ERROR] Build failed. Deployment aborted.
    pause
    exit /b %errorlevel%
)

:: 2. Package with Velopack
echo [PACKAGE] Creating Velopack installer...
:: Update the path to vpk if it's not in your PATH
vpk pack -u "OrangeCircleConstruction" -p publish -e "OCC.Client.Desktop.exe" --packTitle "Orange Circle Construction" --packAuthors "Origize63" -v %VERSION%

if %errorlevel% neq 0 (
    echo [ERROR] Packaging failed.
    pause
    exit /b %errorlevel%
)

echo [SUCCESS] Client version %VERSION% packaged successfully!
echo Setup file can be found in the 'releases' folder.
pause
endlocal
