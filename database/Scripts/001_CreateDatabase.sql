/*
    CoilManager database bootstrap placeholder.
    Business tables are intentionally deferred until the relevant module batches.
*/

IF DB_ID(N'CoilManager') IS NULL
BEGIN
    CREATE DATABASE [CoilManager];
END;
GO
