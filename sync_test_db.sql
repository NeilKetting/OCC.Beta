-- =============================================
-- CLONE LIVE DATABASE TO TEST
-- =============================================
USE master;
GO

DECLARE @LiveDbName NVARCHAR(100) = N'OCC_Rev5_DB';
DECLARE @TestDbName NVARCHAR(100) = N'OCC_Test';
DECLARE @BackupPath NVARCHAR(500) = N'C:\OCCBackups\OCC_Live_Temp.bak';
DECLARE @TestDataPath NVARCHAR(500) = N'C:\Program Files\Microsoft SQL Server\MSSQL17.OCC_SQL\MSSQL\DATA\OCC_Test.mdf';
DECLARE @TestLogPath NVARCHAR(500) = N'C:\Program Files\Microsoft SQL Server\MSSQL17.OCC_SQL\MSSQL\DATA\OCC_Test_log.ldf';

-- 1. Kick everyone off Staging DB (only if it exists)
IF EXISTS (SELECT name FROM sys.databases WHERE name = @TestDbName)
BEGIN
    PRINT 'Setting Test DB to Single User Mode...';
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'ALTER DATABASE [' + @TestDbName + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE';
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'Test DB does not exist yet. It will be created by the restore process.';
END

-- 2. Backup Live
PRINT 'Backing up Live DB...';
-- Enable explicit transaction handling for safety
SET XACT_ABORT ON;

BACKUP DATABASE @LiveDbName 
TO DISK = @BackupPath 
WITH FORMAT, INIT, CHECKSUM, STATS = 10, NAME = N'Full Backup of Live for Test Sync';

-- 3. Restore to Test (This creates the DB if it doesn't exist)
PRINT 'Restoring to Test DB...';
DECLARE @LiveLogName NVARCHAR(128);
SET @LiveLogName = @LiveDbName + N'_log';

RESTORE DATABASE @TestDbName 
FROM DISK = @BackupPath 
WITH REPLACE, RECOVERY, CHECKSUM, STATS = 10,
MOVE @LiveDbName TO @TestDataPath,
MOVE @LiveLogName TO @TestLogPath;

-- 4. Bring Test back online
PRINT 'Setting Test DB back to Multi User Mode...';
DECLARE @sqlMulti NVARCHAR(MAX);
SET @sqlMulti = N'ALTER DATABASE [' + @TestDbName + N'] SET MULTI_USER';
EXEC sp_executesql @sqlMulti;

PRINT 'Synchronization Complete!';
GO
