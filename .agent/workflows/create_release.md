---
description: Create a new release installer for the Desktop Client
---

This workflow compiles the latest code in Release mode and packages it into a standard Velopack installer.

1. **Publish Clean Binaries**
   First, we must compile the code and output it to the `publish` folder. This ensures the installer contains the latest changes.
   
```powershell
dotnet publish OCC.Client/OCC.Client/OCC.Client.csproj -c Release -o publish
```

// turbo
2. **Pack Installer**
   Then, we use `vpk` to wrap those binaries into an installer. 
   *Note: Update the `-v` version number as needed.*

```powershell
vpk pack -u "OrangeCircleConstruction" -p publish -e "OCC.Client.Desktop.exe" --packTitle "Orange Circle Construction" --packAuthors "Origize63" -v 1.4.4
```
