---
description: Create a new STAGING release for the Desktop Client
---

This workflow compiles the latest code with the `STAGING` flag and packages it as a separate application ("OCC Staging").

**IMPORTANT: Run these commands from the Repository Root**

1. **Publish Staging Binaries**
   We compile the code specifically for Staging. This will activate the **port 8082** API URL.
   
```powershell
dotnet publish OCC.Client/OCC.Client.Desktop/OCC.Client.Desktop.csproj -c Release -o publish_staging /p:DefineConstants="STAGING"
```

2. **Pack Staging Installer**
   We use `vpk` to wrap these binaries. We use a unique ID (`OCC-Staging`) and title to ensure it doesn't overwrite your Live app.

```powershell
vpk pack -u "OCC-Staging" -p publish_staging -e "OCC.Client.Desktop.exe" --packTitle "OCC Staging" --packAuthors "Origize63" -v 1.4.5-staging -o staging-releases
```

3. **Verification**
   - Run the generated `OCC-Staging-Setup.exe`.
   - You should see "OCC Staging" in your Start Menu.
   - It will automatically talk to the Staging Database (port 8082).
