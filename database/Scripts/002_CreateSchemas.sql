/*
    CoilManager schema bootstrap placeholder.
    Keep schema creation idempotent so scripts can be rerun safely in local environments.
*/

IF SCHEMA_ID(N'app') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [app]');
END;
GO

IF SCHEMA_ID(N'identity') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [identity]');
END;
GO
