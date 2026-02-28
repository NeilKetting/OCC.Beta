-- Delete today's Auto Clock-In/Out records (e.g., if it ran on a weekend)
-- Run this script in SSMS against the live database

DELETE FROM AttendanceRecords
WHERE IsAutoClockIn = 1
AND CAST([Date] AS DATE) = CAST(GETDATE() AS DATE);
