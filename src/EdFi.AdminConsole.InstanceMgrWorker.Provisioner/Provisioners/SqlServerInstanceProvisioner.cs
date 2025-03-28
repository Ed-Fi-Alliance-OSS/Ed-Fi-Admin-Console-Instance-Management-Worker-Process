// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Data.Common;
using System.Runtime.InteropServices;
using Dapper;
using EdFi.Admin.DataAccess.Utils;
using EdFi.Ods.Common.Configuration;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners;

public class SqlServerInstanceProvisioner : InstanceProvisionerBase
{
    private readonly ILog _logger = LogManager.GetLogger(typeof(SqlServerInstanceProvisioner));
    private SqlServerHostPlatform? _sqlServerHostPlatform;
    private readonly string _sqlServerBakFile;

    public SqlServerInstanceProvisioner(IConfiguration configuration,
            IConfigConnectionStringsProvider connectionStringsProvider, IDatabaseNameBuilder databaseNameBuilder)
            : base(configuration, connectionStringsProvider, databaseNameBuilder)
    {
        _sqlServerBakFile = configuration.GetSection("AppSettings:SqlServerBakFile").Value ?? string.Empty;
    }

    public override async Task<bool> CheckDatabaseExists(string instanceName)
    {
        using (var conn = CreateConnection())
        {
            var results = await conn.QueryAsync<string>(
                    $"SELECT name FROM sys.databases WHERE name like @DbName;",
                    new { DbName = _databaseNameBuilder.SandboxNameForKey(instanceName) }, commandTimeout: CommandTimeout)
                .ConfigureAwait(false);

            return (results?.ToArray().Length ?? 0) > 0;
        }
    }

    public override async Task CopyDbInstanceAsync(string originalDatabaseName, string newDatabaseName)
    {
        using (var conn = CreateConnection())
        {
            try
            {
                await conn.OpenAsync();

                string sqlServerBakDirectoryMessage = $"backup directory = {_sqlServerBakFile}";
                _logger.Debug(sqlServerBakDirectoryMessage);

                string backup = _sqlServerBakFile;

                string sqlServerBakFileMessage = $"backup file = {_sqlServerBakFile}";
                _logger.Debug(sqlServerBakFileMessage);

                var sqlFileInfo = await GetDatabaseFilesAsync(newDatabaseName)
                    .ConfigureAwait(false);

                await Restore()
                    .ConfigureAwait(false);

                // NOTE: these helper functions are using the same connection now.
                async Task Restore()
                {
                    string logicalNameForRows = "", logicalNameForLog = "";

                    string restoringFilesFromMessage = $"restoring files from {backup}.";
                    _logger.Debug(restoringFilesFromMessage);

                    using (var reader = await conn.ExecuteReaderAsync($@"RESTORE FILELISTONLY FROM DISK = '{backup}';", commandTimeout: CommandTimeout)
                        .ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            string logicalName = reader.GetString(0);
                            string Type = reader.GetString(2);
                            if (Type.Equals(LogicalNameType.D.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                logicalNameForRows = logicalName;
                            }
                            else if (Type.Equals(LogicalNameType.L.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                logicalNameForLog = logicalName;
                            }
                        }

                        string logicalNameForRowsMessage = $"logical name for Rows Type = {logicalNameForRows}";
                        _logger.Debug(logicalNameForRowsMessage);
                        string logicalNameForLogMessage = $"logical name for Log Type = {logicalNameForLog}";
                        _logger.Debug(logicalNameForLogMessage);
                    }

                    string restoringDatabaseMessage = $"Restoring database {newDatabaseName} from {backup}";
                    _logger.Debug(restoringDatabaseMessage);

                    await conn.ExecuteAsync(
                            $@"RESTORE DATABASE [{newDatabaseName}] FROM DISK = '{backup}' WITH REPLACE, MOVE '{logicalNameForRows}' TO '{sqlFileInfo.Data}', MOVE '{logicalNameForLog}' TO '{sqlFileInfo.Log}';", commandTimeout: CommandTimeout)
                        .ConfigureAwait(false);

                    var changeLogicalDataName = $"ALTER DATABASE[{newDatabaseName}] MODIFY FILE(NAME='{logicalNameForRows}', NEWNAME='{newDatabaseName}')";
                    await conn.ExecuteAsync(changeLogicalDataName, commandTimeout: CommandTimeout)
                              .ConfigureAwait(false);

                    var changeLogicalLogName = $"ALTER DATABASE[{newDatabaseName}] MODIFY FILE(NAME='{logicalNameForLog}', NEWNAME='{newDatabaseName}_log')";
                    await conn.ExecuteAsync(changeLogicalLogName, commandTimeout: CommandTimeout)
                              .ConfigureAwait(false);
                }

                async Task<SqlFileInfo> GetDatabaseFilesAsync(string newName)
                {
                    var hostPlatform = await GetSqlServerHostPlatform();

                    if (hostPlatform == null)
                        throw new InvalidOperationException();
                    else
                    {
                        if (hostPlatform.OsPlatform.Equals(OSPlatform.Windows))
                        {
                            var sqlData = $"SELECT physical_name AS DefaultDataPath FROM sys.master_files WHERE type = {(int)DataPathType.Data} AND database_id = 1;";
                            var sqlLog = $"SELECT physical_name AS DefaultLogPath FROM sys.master_files WHERE type = {(int)DataPathType.Log} AND database_id = 1;";

                            string? fullPathData = await conn.ExecuteScalarAsync<string>(sqlData, commandTimeout: CommandTimeout)
                                .ConfigureAwait(false);

                            string? fullPathLog = await conn.ExecuteScalarAsync<string>(sqlLog, commandTimeout: CommandTimeout)
                                .ConfigureAwait(false);

                            return new SqlFileInfo
                            {
                                Data = fullPathData?.Replace("master", newName),
                                Log = fullPathLog?.Replace("mastlog", $"{newName}_Log"),
                            };
                        }
                        else if (hostPlatform.OsPlatform.Equals(OSPlatform.Linux))
                        {
                            return new SqlFileInfo
                            {
                                Data = $"/var/opt/mssql/data/{newName}.mdf",
                                Log = $"/var/opt/mssql/data/{newName}.ldf"
                            };
                        }
                        else
                            throw new InvalidOperationException();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw new Exception(e.Message);
            }
        }
    }

    public override async Task DeleteDbInstancesAsync(params string[] deletedClientKeys)
    {
        using (var conn = CreateConnection())
        {
            foreach (string key in deletedClientKeys)
            {
                await conn.ExecuteAsync($@"
                         IF EXISTS (SELECT name from sys.databases WHERE (name = '{_databaseNameBuilder.SandboxNameForKey(key)}'))
                        BEGIN
                            ALTER DATABASE [{_databaseNameBuilder.SandboxNameForKey(key)}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [{_databaseNameBuilder.SandboxNameForKey(key)}];
                        END;
                        ", commandTimeout: CommandTimeout)
                    .ConfigureAwait(false);
            }
        }
    }

    public override Task<InstanceStatus> GetDbInstanceStatusAsync(string clientKey)
    {
        throw new NotImplementedException();
    }

    public override Task<string[]> GetInstancesDatabasesAsync()
    {
        throw new NotImplementedException();
    }

    public override async Task RenameDbInstancesAsync(string oldName, string newName)
    {
        using (var conn = CreateConnection())
        {
            await conn.ExecuteAsync($"ALTER DATABASE {oldName} MODIFY Name = {newName};")
                .ConfigureAwait(false);
        }
    }

    protected override DbConnection CreateConnection() => new SqlConnection(ConnectionString);

    private async Task<SqlServerHostPlatform> GetSqlServerHostPlatform()
    {
        if (_sqlServerHostPlatform is null)
        {
            using (var conn = CreateConnection())
            {
                // Get SQL Server OS; sys.dm_os_host_info was introduced in SQL Server 2017 (alongside Linux support)
                // if the table doesn't exist we can assume that the OS is Windows

                const string Windows = "Windows";

                var getHostPlatformSql = $"IF OBJECT_ID('sys.dm_os_host_info') IS NOT NULL SELECT host_platform FROM sys.dm_os_host_info ELSE SELECT '{Windows}'";

                string? hostPlatform = await conn.ExecuteScalarAsync<string>(getHostPlatformSql, commandTimeout: CommandTimeout)
                    .ConfigureAwait(false);

                var isWindows = !(hostPlatform is not null) || hostPlatform.Equals(Windows, StringComparison.InvariantCultureIgnoreCase);

                _sqlServerHostPlatform = new SqlServerHostPlatform()
                {
                    OsPlatform = isWindows ? OSPlatform.Windows : OSPlatform.Linux,
                    DirectorySeparatorChar = isWindows ? '\\' : '/',
                    VolumeSeparatorChar = isWindows ? ':' : '/'
                };
            }
        }

        return _sqlServerHostPlatform;
    }

    private sealed class SqlFileInfo
    {
        public string? Data { get; set; }

        public string? Log { get; set; }
    }

    private sealed class SqlServerHostPlatform
    {
        public OSPlatform OsPlatform { get; set; }

        public char DirectorySeparatorChar { get; set; }

        public char VolumeSeparatorChar { get; set; }
    }

    private enum DataPathType
    {
        Data = 0,
        Log = 1
    }

    private enum LogicalNameType
    {
        D,
        L
    }
}

