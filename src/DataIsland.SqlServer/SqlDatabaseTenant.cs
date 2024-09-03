﻿namespace DataIsland.SqlServer;

public record SqlDatabaseTenant(string ConnectionString, SqlServerComponent Component, string DatabaseName)
    : AdoNetDatabaseTenant(ConnectionString, DatabaseName);
