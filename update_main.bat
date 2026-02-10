@echo off
setlocal

:: Ensure we are running from the script's own directory
cd /d "%~dp0"

echo ========================================================
echo [DEPLOY] MAIN DEPLOYMENT AND SYNC AUTOMATION
echo ========================================================

:: 1. Optional Database Sync
set /p SYNC="Sync Live DB to Main 1:1 before deployment? (Y/N): "
if /i "%SYNC%"=="Y" (
    echo [SYNC] Starting Database Synchronization...
    :: Use full path to the sql file to avoid "Invalid filename" errors
    sqlcmd -b -S "OCOR\OCC_SQL" -i "%~dp0sync_main_db.sql"
    if %errorlevel% neq 0 (
        echo [ERROR] Database Sync failed. Deployment aborted.
        pause
        exit /b %errorlevel%
    )
    echo [SYNC] Database Synchronization Successful.
)

:: 2. Stop IIS (Requires Elevation)
echo [DEPLOY] Stopping IIS Site...
%windir%\system32\inetsrv\appcmd stop site /site.name:"OCC-API"

echo [DEPLOY] Stopping IIS AppPool...
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"OCC-API"

echo Waiting for process to release locks...
timeout /t 5 /nobreak

:: 3. Update Code
echo [DEPLOY] Pulling latest code from MASTER branch...
:: Ensure we are in the repo root
if not exist ".git" (
    echo [ERROR] Not in a git repository. Checked: %cd%
    pause
    exit /b 1
)
git pull origin master

:: 4. Publish
echo [DEPLOY] Publishing to Live Folder...
dotnet publish "OCC.API\OCC.API.csproj" -c Release -o "C:\inetpub\wwwroot\OCC-API"

:: 5. Restart IIS
echo [DEPLOY] Starting IIS AppPool...
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"OCC-API"

echo [DEPLOY] Starting IIS Site...
%windir%\system32\inetsrv\appcmd start site /site.name:"OCC-API"

echo [DEPLOY] Update Complete!
pause
endlocal
