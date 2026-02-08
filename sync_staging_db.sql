-- =============================================
-- CLONE LIVE DATABASE TO STAGING
-- =============================================
USE master;
GO

DECLARE @LiveDbName NVARCHAR(100) = N'OCC_Rev5_DB';
DECLARE @StagingDbName NVARCHAR(100) = N'OCC_Staging';
DECLARE @BackupPath NVARCHAR(500) = N'C:\OCCBackups\OCC_Live_Temp.bak';
DECLARE @StagingDataPath NVARCHAR(500) = N'C:\Program Files\Microsoft SQL Server\MSSQL17.OCC_SQL\MSSQL\DATA\OCC_Staging.mdf';
DECLARE @StagingLogPath NVARCHAR(500) = N'C:\Program Files\Microsoft SQL Server\MSSQL17.OCC_SQL\MSSQL\DATA\OCC_Staging_log.ldf';

-- 1. Kick everyone off Staging DB (only if it exists)
IF EXISTS (SELECT name FROM sys.databases WHERE name = @StagingDbName)
BEGIN
    PRINT 'Setting Staging DB to Single User Mode...';
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'ALTER DATABASE [' + @StagingDbName + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE';
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'Staging DB does not exist yet. It will be created by the restore process.';
END

-- 2. Backup Live
PRINT 'Backing up Live DB...';
BACKUP DATABASE @LiveDbName 
TO DISK = @BackupPath 
WITH FORMAT, INIT, NAME = N'Full Backup of Live for Staging Sync';

-- 3. Restore to Staging (This creates the DB if it doesn't exist)
PRINT 'Restoring to Staging DB...';
DECLARE @LiveLogName NVARCHAR(128);
SET @LiveLogName = @LiveDbName + N'_log';

RESTORE DATABASE @StagingDbName 
FROM DISK = @BackupPath 
WITH REPLACE, 
MOVE @LiveDbName TO @StagingDataPath,
MOVE @LiveLogName TO @StagingLogPath;

-- 4. Bring Staging back online
PRINT 'Setting Staging DB back to Multi User Mode...';
DECLARE @sqlMulti NVARCHAR(MAX);
SET @sqlMulti = N'ALTER DATABASE [' + @StagingDbName + N'] SET MULTI_USER';
EXEC sp_executesql @sqlMulti;

PRINT 'Synchronization Complete!';
GO
