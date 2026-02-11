# OCC System Intelligence & Architecture Guide

This guide is intended for AI assistants and developers to quickly understand the OCC project structure and environment.

## 1. Environment Topology

### **Local Development (Neil's PC)**
- **Role**: Source of Truth for code.
- **Tools**: Visual Studio, Git.
- **Database**: Local SQL instances.
- **Access**: Full filesystem access.

### **Hosted Server (Production)**
- **IP**: `102.221.36.149` (Port `8081` for API)
- **Repo Location**: `C:\OCC-Source`
- **Publish Location**: `C:\inetpub\wwwroot\OCC_API`
- **Database**: SQL Server `OCOR\OCC_SQL`
- **Access Level**:
    - **Filesystem**: NOT directly accessible from the local PC via this agent.
    - **Logs**: Accessible via `GET http://102.221.36.149:8081/api/Logs`
    - **Deployment**: Managed by `update_main.bat` on the server.

---

## 2. API Architecture & Routing

### **Standard Patterns**
- **Base Route**: `api/[ControllerName]`
- **HTTP Methods**: ALWAYS use standard Restful methods.
    - `GET`: Retrieve data.
    - `POST`: Create new records (`api/AttendanceRecords`).
    - `PUT`: Update records (`api/AttendanceRecords/{id}`).
    - `DELETE`: Delete records (`api/AttendanceRecords/{id}`).
- **Auth**: JWT Bearer tokens. Header: `Authorization: Bearer <token>`.

### **Environment Sync (Crucial)**
Client services MUST be dynamic. They use `ConnectionSettings.Instance.ApiBaseUrl`. 
Singletons like `ApiAuthService` and `SignalRNotificationService` must handle updates to this URL when the user switches between "Local" and "Live" environments in the UI.

---

## 3. Maintenance & Deployment

### **Deployment Script (`update_main.bat`)**
- Uses `git stash` to protect local server changes (like `appsettings.secrets.json`).
- Always performs `git pull origin master`.
- Uses `dotnet publish` to update the IIS directory.
- **Batch Syntax**: Use `REM` for comments inside `if/else` blocks; `::` can cause crashes.

### **SDK & Versioning**
- Managed by `global.json`. 
- **Minimum SDK**: `9.0.100` (pinned to ensure server compatibility).

---

## 4. Feature Map
- **Employee Hub**: Attendance, Time Tracking, Leave, Overtime.
- **Projects Hub**: Tasks, Assignments, Gantt charts.
- **Bug Hub**: Performance-optimized bug reporting.
- **Auth Hub**: JWT-based login and environment switching.
