@echo off
setlocal

:: Ensure we are running from the script's own directory
cd /d "%~dp0"

echo ========================================================
echo [TEST] DEPLOYMENT AND SYNC AUTOMATION
echo ========================================================

:: 1. Optional Database Sync
set /p SYNC="Sync Live DB to Test 1:1 before deployment? (Y/N): "
if /i "%SYNC%"=="Y" (
    echo [SYNC] Starting Database Synchronization...
    :: Use full path to the sql file to avoid "Invalid filename" errors
    sqlcmd -b -S "OCOR\OCC_SQL" -i "%~dp0sync_test_db.sql"
    if %errorlevel% neq 0 (
        echo [ERROR] Database Sync failed. Deployment aborted.
        pause
        exit /b %errorlevel%
    )
    echo [SYNC] Database Synchronization Successful.
)

:: 2. Stop IIS (Requires Elevation)
echo [TEST] Stopping IIS Site...
%windir%\system32\inetsrv\appcmd stop site /site.name:"OCC-API-Staging"

echo [TEST] Stopping IIS AppPool...
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"OCC-API-Staging"

echo Waiting for process to release locks...
timeout /t 5 /nobreak

:: 3. Update Code
echo [TEST] Pulling latest code from MASTER branch...
:: Ensure we are in the repo root
if not exist ".git" (
    echo [ERROR] Not in a git repository. Checked: %cd%
    pause
    exit /b 1
)
git pull origin master

:: 4. Publish
echo [TEST] Publishing to Test Folder...
dotnet publish "OCC.API\OCC.API.csproj" -c Release -o "C:\inetpub\wwwroot\OCC-API-Staging"

:: 5. Restart IIS
echo [TEST] Starting IIS AppPool...
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"OCC-API-Staging"

echo [TEST] Starting IIS Site...
%windir%\system32\inetsrv\appcmd start site /site.name:"OCC-API-Staging"

echo [TEST] Update Complete!
pause
endlocal
