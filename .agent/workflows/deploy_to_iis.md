---
description: Deploying OCC API to IIS on Windows Server
---
# How to Host OCC.API on IIS

This guide assumes you are deploying to a dedicated Windows PC/Server.

## 1. Prepare Your Local Code
1.  **Commit & Push**: Ensure all your local changes are committed and pushed to your GitHub repository.
    ```powershell
    git add .
    git commit -m "Ready for deployment"
    git push origin main
    ```

## 2. Server Prerequisites (Do this once on the Hosted PC)
1.  **Install .NET 9 Hosting Bundle**:
    *   Download and install the **Asp.Net Core Runtime 9.0 (Hosting Bundle)** from Microsoft.
    *   This installs the IIS Module required to run .NET apps.
2.  **Install .NET 9 SDK**:
    *   **Crucial**: To build/publish the app on the server (which this guide assumes), you MUST also install the **.NET 9 SDK (x64)**. The Hosting Bundle only includes the Runtime.
3.  **Enable IIS**:
    *   Open "Turn Windows features on or off".
    *   Ensure **Internet Information Services** is checked.
    *   Under `World Wide Web Services` -> `Application Development Features`, ensure **.NET Extensibility 4.8** and **ASP.NET 4.8** are checked (good measure).
3.  **Install Git**: Download Git for Windows so you can pull your code.

## 3. Deployment Steps (The "Git Pull" Method)
Perform these steps on the **Hosted PC**:

### A. First Time Setup
1.  **Clone Repo**:
    ```powershell
    cd C:\
    git clone https://github.com/YourUsername/OCC-Rev5.git C:\OCC-Source
    ```
2.  **Create Publish Folder**:
    *   Create a folder where the live app will live, e.g., `C:\inetpub\wwwroot\OCC-API`.
3.  **Configure IIS**:
    *   Open **IIS Manager** (Start -> Run -> `inetmgr`).
    *   Right-click **Sites** -> **Add Website**.
    *   **Site name**: `OCC-API`
    *   **Physical path**: `C:\inetpub\wwwroot\OCC-API`
    *   **Port**: `80` (or `7166` if you want to match dev, but 80 is standard).
    *   **Host name**: Leave blank (or set to your static IP/domain).
    *   Click OK.
4.  **Application Pool**:
    *   Click **Application Pools**.
    *   Double-click `OCC-API`.
    *   Set **.NET CLR Version** to `No Managed Code`.

### B. Deploy/Update Script
Create a file `C:\OCC-Source\update_api.bat` with the following content:

```batch
@echo off
echo Stopping IIS Site...
%windir%\system32\inetsrv\appcmd stop site /site.name:"OCC_API"

echo Stopping IIS AppPool...
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"OCC_API"

echo Waiting for process to release locks...
timeout /t 5 /nobreak

echo Pulling latest code...
cd C:\OCC-Source
git pull origin master

echo Publishing...
dotnet publish "OCC.API\OCC.API.csproj" -c Release -o "C:\inetpub\wwwroot\OCC-API"

echo Starting IIS AppPool...
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"OCC_API"

echo Starting IIS Site...
%windir%\system32\inetsrv\appcmd start site /site.name:"OCC_API"

echo Done!
pause
```

## 4. Run as Administrator
**IMPORTANT**: You must right-click this `.bat` file and select **Run as administrator**. "appcmd" requires admin rights to stop/start sites.

## 5. Troubleshooting
*   **Error: "No .NET SDKs were found"**: Install the .NET 9 SDK.
*   **Error: "Insufficient permissions"**: Run the batch file as Administrator.
*   **Error: "couldn't find remote ref main"**: Your branch is likely named `master`. The script above uses `master`.
