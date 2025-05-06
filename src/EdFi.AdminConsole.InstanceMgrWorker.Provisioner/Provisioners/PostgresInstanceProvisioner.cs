// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Data.Common;
using Dapper;
using EdFi.Admin.DataAccess.Utils;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners
{
    public class PostgresInstanceProvisioner : InstanceProvisionerBase
    {
        public PostgresInstanceProvisioner(IConfiguration configuration,
            IMgrWorkerConfigConnectionStringsProvider connectionStringsProvider, IMgrWorkerIDatabaseNameBuilder databaseNameBuilder)
            : base(configuration, connectionStringsProvider, databaseNameBuilder) { }

        public override async Task RenameDbInstancesAsync(string oldName, string newName)
        {
            using (var conn = CreateConnection())
            {
                string sql = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{oldName}';";

                await conn.ExecuteAsync(sql, commandTimeout: CommandTimeout)
                    .ConfigureAwait(false);

                sql = $"ALTER DATABASE \"{oldName}\" RENAME TO \"{newName}\";";

                await conn.ExecuteAsync(sql, commandTimeout: CommandTimeout)
                    .ConfigureAwait(false);
            }
        }

        public override async Task DeleteDbInstancesAsync(params string[] deletedClientKeys)
        {
            using (var conn = CreateConnection())
            {
                foreach (string key in deletedClientKeys)
                {
                    await conn.ExecuteAsync(
                        $@"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{_databaseNameBuilder.OdsDatabaseName(_tenant, key)}';");

                    await conn.ExecuteAsync(
                            $@"DROP DATABASE IF EXISTS ""{_databaseNameBuilder.OdsDatabaseName(_tenant, key)}"";",
                            commandTimeout: CommandTimeout)
                        .ConfigureAwait(false);
                }
            }
        }

        public override async Task CopyDbInstanceAsync(string originalDatabaseName, string newDatabaseName)
        {
            using (var conn = CreateConnection())
            {
                string sql = @$"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{originalDatabaseName}';";
                await conn.ExecuteAsync(sql, commandTimeout: CommandTimeout).ConfigureAwait(false);

                sql = @$"CREATE DATABASE ""{newDatabaseName}"" TEMPLATE ""{originalDatabaseName}""";
                await conn.ExecuteAsync(sql, commandTimeout: CommandTimeout).ConfigureAwait(false);
            }
        }

        protected override DbConnection CreateConnection() => new NpgsqlConnection(ConnectionString);

        public override async Task<InstanceStatus> GetDbInstanceStatusAsync(string clientKey)
        {
            using (var conn = CreateConnection())
            {
                var results = await conn.QueryAsync<InstanceStatus>(
                        $"SELECT datname as Name, 0 as Code, 'ONLINE' Description FROM pg_database WHERE datname = \'{_databaseNameBuilder.OdsDatabaseName(_tenant, clientKey)}\';",
                        commandTimeout: CommandTimeout)
                    .ConfigureAwait(false);

                return results.SingleOrDefault() ?? InstanceStatus.ErrorStatus();
            }
        }

        public override async Task<string[]> GetInstancesDatabasesAsync()
        {
            using (var conn = CreateConnection())
            {
                var results = await conn.QueryAsync<string>(
                        $"SELECT datname as name FROM pg_database WHERE datname like \'{_databaseNameBuilder.OdsDatabaseName(_tenant, "%")}\';",
                        commandTimeout: CommandTimeout)
                    .ConfigureAwait(false);

                return results.ToArray();
            }
        }

        public override async Task<bool> CheckDatabaseExists(string instanceName)
        {
            using (var conn = CreateConnection())
            {
                var results = await conn.QueryAsync<string>(
                        $"SELECT datname as name FROM pg_database WHERE datname like \'{_databaseNameBuilder.OdsDatabaseName(_tenant, instanceName)}\';",
                        commandTimeout: CommandTimeout)
                    .ConfigureAwait(false);

                return (results?.ToArray().Length ?? 0) > 0;
            }
        }
    }
}
