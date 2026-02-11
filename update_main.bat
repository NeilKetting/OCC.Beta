@echo off
setlocal

REM Ensure we are running from the script's own directory
cd /d "%~dp0"

echo ========================================================
echo [DEPLOY] MAIN DEPLOYMENT AND SYNC AUTOMATION
echo ========================================================
echo [DEBUG] Current Directory: %cd%
echo.

REM 1. Optional Database Sync
set /p SYNC="Sync Live DB to Main 1:1 before deployment? (Y/N): "
if /i "%SYNC%"=="Y" (
    echo [SYNC] Starting Database Synchronization...
    REM Use full path to the sql file to avoid "Invalid filename" errors
    if exist "%~dp0sync_main_db.sql" (
        sqlcmd -b -S "OCOR\OCC_SQL" -i "%~dp0sync_main_db.sql"
        if %errorlevel% neq 0 (
            echo [ERROR] Database Sync failed. 
            pause
            exit /b %errorlevel%
        )
        echo [SYNC] Database Synchronization Successful.
    ) else (
        echo [ERROR] sync_main_db.sql not found. Skipping sync.
    )
)

REM 2. Stop IIS (Requires Elevation)
echo [DEPLOY] Stopping IIS Site...
%windir%\system32\inetsrv\appcmd stop site /site.name:"OCC_API"
if %errorlevel% neq 0 echo [WARN] Could not stop site. It might be already stopped.

echo [DEPLOY] Stopping IIS AppPool...
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"OCC_API"
if %errorlevel% neq 0 echo [WARN] Could not stop apppool. It might be already stopped.

echo Waiting for process to release locks...
timeout /t 5 /nobreak

REM 3. Update Code
echo [DEPLOY] Verifying Git Repository...
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

echo [DEPLOY] Stashing local changes...
git stash push -m "Deployment_auto-stash"
echo [DEBUG] Stash result code: %errorlevel%

echo [DEPLOY] Fetching latest changes...
git fetch origin master
echo [DEBUG] Fetch result code: %errorlevel%

echo [DEPLOY] Pulling changes from MASTER...
git pull origin master
echo [DEBUG] Pull finished. Code: %errorlevel%
pause

REM Try to restore stashed changes
echo [DEPLOY] Restoring local changes...
git stash pop
if %errorlevel% neq 0 (
    echo [WARN] Conflict or No Stash found during pop. Code: %errorlevel%
    
    REM Specifically try to keep local appsettings.json if it was stashed
    echo [DEPLOY] Ensuring appsettings.json is clean...
    git checkout --ours OCC.API/appsettings.json 2>nul
    
    git stash drop 2>nul
)
echo [DEBUG] Post-stash cleanup done.
pause

REM 4. Publish
echo [DEPLOY] Publishing to Live Folder...
if exist "OCC.API\OCC.API.csproj" (
    dotnet publish "OCC.API\OCC.API.csproj" -c Release -o "C:\inetpub\wwwroot\OCC_API"
    if %errorlevel% neq 0 (
        echo [ERROR] Publish failed!
        pause
    )
) else (
    echo [ERROR] OCC.API\OCC.API.csproj NOT FOUND!
    pause
)

REM 5. Restart IIS
echo [DEPLOY] Starting IIS AppPool...
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"OCC_API"

echo [DEPLOY] Starting IIS Site...
%windir%\system32\inetsrv\appcmd start site /site.name:"OCC_API"

echo [DEPLOY] Update Complete!
pause
endlocal
