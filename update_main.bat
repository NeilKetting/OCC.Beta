@echo off
setlocal

REM Ensure we are running from the script's own directory
cd /d "%~dp0"

echo ========================================================
echo [DEPLOY] MAIN DEPLOYMENT AND SYNC AUTOMATION
echo ========================================================
echo.

REM 1. Optional Database Sync
set /p SYNC="Sync Live DB to Main 1:1 before deployment? (Y/N): "
if /i "%SYNC%"=="Y" (
    echo [SYNC] Starting Database Synchronization...
    REM Use full path to the sql file to avoid "Invalid filename" errors
    sqlcmd -b -S "OCOR\OCC_SQL" -i "%~dp0sync_main_db.sql"
    if %errorlevel% neq 0 (
        echo [ERROR] Database Sync failed. Deployment aborted.
        pause
        exit /b %errorlevel%
    )
    echo [SYNC] Database Synchronization Successful.
)

REM 2. Stop IIS (Requires Elevation)
echo [DEPLOY] Stopping IIS Site...
%windir%\system32\inetsrv\appcmd stop site /site.name:"OCC_API"

echo [DEPLOY] Stopping IIS AppPool...
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"OCC_API"

echo Waiting for process to release locks...
timeout /t 5 /nobreak

REM 3. Update Code
echo [DEPLOY] Verifying Remote URL...
REM Ensure we are in the repo root
if not exist ".git" (
    echo [ERROR] Not in a git repository. Checked: %cd%
    pause
    exit /b 1
)

REM Check if remote is pointing to OCC.Beta
git remote get-url origin | findstr /i "OCC.Beta" >nul
if %errorlevel% neq 0 (
    echo [WARN] Remote 'origin' is not pointing to OCC.Beta. Updating...
    git remote set-url origin https://github.com/NeilKetting/OCC.Beta.git
    echo [INFO] Remote updated to OCC.Beta.
)

echo [DEPLOY] Pulling latest code from MASTER branch...
REM Stash ALL local changes to ensure a clean pull
echo [DEPLOY] Stashing local changes...
git stash push -m "Deployment_auto-stash"

echo [DEPLOY] Fetching latest changes...
git fetch origin master

echo [DEPLOY] Pulling changes...
git pull origin master --no-edit

REM Try to restore stashed changes
echo [DEPLOY] Restoring local changes...
git stash pop
if %errorlevel% neq 0 (
    echo [WARN] Conflict detected during stash pop. 
    echo [WARN] This usually means your local config (like appsettings.json) conflicted with new code.
    echo [WARN] Attempting to keep your local versions of config files...
    
    REM Specifically try to keep local appsettings.json if it was stashed
    git checkout --ours OCC.API/appsettings.json 2>nul
    
    REM If there are still conflicts, you may need to resolve them manually.
    echo [INFO] Conflicts handled where possible. If build fails, check for .merge files.
    git stash drop
)

REM 4. Publish
echo [DEPLOY] Publishing to Live Folder...
dotnet publish "OCC.API\OCC.API.csproj" -c Release -o "C:\inetpub\wwwroot\OCC_API"

REM 5. Restart IIS
echo [DEPLOY] Starting IIS AppPool...
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"OCC_API"

echo [DEPLOY] Starting IIS Site...
%windir%\system32\inetsrv\appcmd start site /site.name:"OCC_API"

echo [DEPLOY] Update Complete!
pause
endlocal
