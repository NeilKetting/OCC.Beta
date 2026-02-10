---
description: Create a new STAGING release for the Desktop Client
---

This workflow compiles the latest code with the `STAGING` flag and packages it as a separate application ("OCC Staging").

**IMPORTANT: Run these commands from the OCC.Client folder**

1. **Create Staging Release**
   This command compiles for staging (port 8082) and packages it into a separate "OCC Staging" app.
   *Note: Update the `-v` version number as needed.*

```powershell
dotnet publish OCC.Client.Desktop/OCC.Client.Desktop.csproj -c Release -o publish_staging /p:DefineConstants="STAGING" --self-contained true -r win-x64; vpk pack -u "OCC-Staging" -p publish_staging -e "OCC.Client.Desktop.exe" --packTitle "OCC Staging" --packAuthors "Origize63" -v 1.4.5-staging -o staging-releases
```

3. **Verification**
   - Run the generated `OCC-Staging-Setup.exe`.
   - You should see "OCC Staging" in your Start Menu.
   - It will automatically talk to the Staging Database (port 8082).
