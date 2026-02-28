-- Export Live Attendance Records (SQL Server 2022 to SQL Server 2019)
-- Run this script in SSMS connected to your *LIVE* database.
-- It will generate INSERT statements for all Attendance Records in the Results pane.
-- To avoid exporting massive amounts of old data, this is filtered to 2026. Adjust the WHERE clause if needed.
-- Copy those results, and run them on your *LOCAL* database (OCC_Dev_DB).

SET NOCOUNT ON;

SELECT 
    'INSERT INTO AttendanceRecords (Id, UserId, EmployeeId, [Date], Status, Latitude, Longitude, CheckInTime, CheckOutTime, HoursWorked, Notes, LeaveReason, DoctorsNoteImagePath, Branch, ClockInTime, CachedHourlyRate, IsActive, CreatedAtUtc, UpdatedAtUtc, CreatedBy, UpdatedBy, IsAutoClockIn) VALUES (' +
    '''' + CAST(Id AS VARCHAR(36)) + ''', ' +
    CASE WHEN UserId IS NULL THEN 'NULL' ELSE '''' + CAST(UserId AS VARCHAR(36)) + '''' END + ', ' +
    CASE WHEN EmployeeId IS NULL THEN 'NULL' ELSE '''' + CAST(EmployeeId AS VARCHAR(36)) + '''' END + ', ' +
    '''' + CONVERT(VARCHAR, [Date], 120) + ''', ' +
    CAST(Status AS VARCHAR(10)) + ', ' +
    CASE WHEN Latitude IS NULL THEN 'NULL' ELSE CAST(Latitude AS VARCHAR(50)) END + ', ' +
    CASE WHEN Longitude IS NULL THEN 'NULL' ELSE CAST(Longitude AS VARCHAR(50)) END + ', ' +
    CASE WHEN CheckInTime IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR, CheckInTime, 120) + '''' END + ', ' +
    CASE WHEN CheckOutTime IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR, CheckOutTime, 120) + '''' END + ', ' +
    CAST(ISNULL(HoursWorked, 0) AS VARCHAR(50)) + ', ' +
    CASE WHEN Notes IS NULL THEN 'NULL' ELSE '''' + REPLACE(Notes, '''', '''''') + '''' END + ', ' +
    CASE WHEN LeaveReason IS NULL THEN 'NULL' ELSE '''' + REPLACE(LeaveReason, '''', '''''') + '''' END + ', ' +
    CASE WHEN DoctorsNoteImagePath IS NULL THEN 'NULL' ELSE '''' + REPLACE(DoctorsNoteImagePath, '''', '''''') + '''' END + ', ' +
    CASE WHEN Branch IS NULL THEN 'NULL' ELSE '''' + REPLACE(Branch, '''', '''''') + '''' END + ', ' +
    CASE WHEN ClockInTime IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR(8), ClockInTime, 108) + '''' END + ', ' +
    CASE WHEN CachedHourlyRate IS NULL THEN 'NULL' ELSE CAST(CachedHourlyRate AS VARCHAR(50)) END + ', ' +
    CAST(IsActive AS VARCHAR(1)) + ', ' +
    '''' + CONVERT(VARCHAR, CreatedAtUtc, 120) + ''', ' +
    CASE WHEN UpdatedAtUtc IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR, UpdatedAtUtc, 120) + '''' END + ', ' +
    CASE WHEN CreatedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(CreatedBy, '''', '''''') + '''' END + ', ' +
    CASE WHEN UpdatedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(UpdatedBy, '''', '''''') + '''' END + ', ' +
    CASE WHEN IsAutoClockIn IS NULL THEN '0' ELSE CAST(IsAutoClockIn AS VARCHAR(1)) END + 
    ');' AS SqlInsertStatement
FROM AttendanceRecords
WHERE [Date] >= '2026-01-01'; -- Filtering for recent data
