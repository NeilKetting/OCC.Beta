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
%windir%\system32\inetsrv\appcmd stop site /site.name:"OCC_API"

echo [DEPLOY] Stopping IIS AppPool...
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"OCC_API"

echo Waiting for process to release locks...
timeout /t 5 /nobreak

:: 3. Update Code
echo [DEPLOY] Verifying Remote URL...
:: Ensure we are in the repo root
if not exist ".git" (
    echo [ERROR] Not in a git repository. Checked: %cd%
    pause
    exit /b 1
)

:: Check if remote is pointing to OCC.Beta
git remote get-url origin | findstr /i "OCC.Beta" >nul
if %errorlevel% neq 0 (
    echo [WARN] Remote 'origin' is not pointing to OCC.Beta. Updating...
    git remote set-url origin https://github.com/NeilKetting/OCC.Beta.git
    echo [INFO] Remote updated to OCC.Beta.
)

echo [DEPLOY] Pulling latest code from MASTER branch...
git pull origin master --no-edit

:: 4. Publish
echo [DEPLOY] Publishing to Live Folder...
dotnet publish "OCC.API\OCC.API.csproj" -c Release -o "C:\inetpub\wwwroot\OCC_API"

:: 5. Restart IIS
echo [DEPLOY] Starting IIS AppPool...
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"OCC_API"

echo [DEPLOY] Starting IIS Site...
%windir%\system32\inetsrv\appcmd start site /site.name:"OCC_API"

echo [DEPLOY] Update Complete!
pause
endlocal
