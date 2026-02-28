-- Export Live Employees (SQL Server 2022 to SQL Server 2019)
-- Run this script in SSMS connected to your *LIVE* database.
-- It will generate INSERT statements for all Active Employees in the Results pane.
-- Copy those results, and run them on your *LOCAL* database (OCC_Dev_DB).

SET NOCOUNT ON;

SELECT 
    'INSERT INTO Employees (Id, LinkedUserId, RateType, HourlyRate, TaxNumber, AnnualLeaveBalance, SickLeaveBalance, LeaveCycleStartDate, FirstName, LastName, IdType, IdNumber, PermitNumber, Email, Phone, DoB, Role, EmployeeNumber, EmploymentType, ContractDuration, EmploymentDate, Branch, ShiftStartTime, ShiftEndTime, BankName, AccountNumber, BranchCode, AccountType, LeaveBalance, Status) VALUES (' +
    '''' + CAST(Id AS VARCHAR(36)) + ''', ' +
    CASE WHEN LinkedUserId IS NULL THEN 'NULL' ELSE '''' + CAST(LinkedUserId AS VARCHAR(36)) + '''' END + ', ' +
    CAST(RateType AS VARCHAR(10)) + ', ' +
    CAST(ISNULL(HourlyRate, 0) AS VARCHAR(20)) + ', ''' +
    REPLACE(ISNULL(TaxNumber, ''), '''', '''''') + ''', ' +
    CAST(ISNULL(AnnualLeaveBalance, 0) AS VARCHAR(20)) + ', ' +
    CAST(ISNULL(SickLeaveBalance, 0) AS VARCHAR(20)) + ', ' +
    CASE WHEN LeaveCycleStartDate IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR, LeaveCycleStartDate, 120) + '''' END + ', ''' +
    REPLACE(ISNULL(FirstName, ''), '''', '''''') + ''', ''' +
    REPLACE(ISNULL(LastName, ''), '''', '''''') + ''', ' +
    CAST(ISNULL(IdType, 0) AS VARCHAR(10)) + ', ''' +
    REPLACE(ISNULL(IdNumber, ''), '''', '''''') + ''', ' +
    CASE WHEN PermitNumber IS NULL THEN 'NULL' ELSE '''' + REPLACE(PermitNumber, '''', '''''') + '''' END + ', ''' +
    REPLACE(ISNULL(Email, ''), '''', '''''') + ''', ''' +
    REPLACE(ISNULL(Phone, ''), '''', '''''') + ''', ''' +
    CONVERT(VARCHAR, ISNULL(DoB, '1900-01-01'), 120) + ''', ' +
    CAST(ISNULL(Role, 0) AS VARCHAR(10)) + ', ''' +
    REPLACE(ISNULL(EmployeeNumber, ''), '''', '''''') + ''', ' +
    CAST(ISNULL(EmploymentType, 0) AS VARCHAR(10)) + ', ' +
    CASE WHEN ContractDuration IS NULL THEN 'NULL' ELSE '''' + REPLACE(ContractDuration, '''', '''''') + '''' END + ', ''' +
    CONVERT(VARCHAR, ISNULL(EmploymentDate, GETDATE()), 120) + ''', ''' +
    REPLACE(ISNULL(Branch, ''), '''', '''''') + ''', ' +
    CASE WHEN ShiftStartTime IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR(8), ShiftStartTime, 108) + '''' END + ', ' +
    CASE WHEN ShiftEndTime IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR(8), ShiftEndTime, 108) + '''' END + ', ' +
    CASE WHEN BankName IS NULL THEN 'NULL' ELSE '''' + REPLACE(BankName, '''', '''''') + '''' END + ', ' +
    CASE WHEN AccountNumber IS NULL THEN 'NULL' ELSE '''' + REPLACE(AccountNumber, '''', '''''') + '''' END + ', ' +
    CASE WHEN BranchCode IS NULL THEN 'NULL' ELSE '''' + REPLACE(BranchCode, '''', '''''') + '''' END + ', ' +
    CASE WHEN AccountType IS NULL THEN 'NULL' ELSE '''' + REPLACE(AccountType, '''', '''''') + '''' END + ', ' +
    CAST(ISNULL(LeaveBalance, 0) AS VARCHAR(20)) + ', ' +
    CAST(ISNULL(Status, 0) AS VARCHAR(10)) + 
    ');' AS SqlInsertStatement
FROM Employees
WHERE Status = 0; -- Active employees
