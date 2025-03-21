// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Data.Common;
using EdFi.Admin.DataAccess.Utils;
using EdFi.Ods.Common.Configuration;
using EdFi.Ods.Common.Extensions;
using Microsoft.Extensions.Configuration;

namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners
{
    public abstract class InstanceProvisionerBase : IInstanceProvisioner
    {
        protected readonly IConfiguration _configuration;
        protected readonly IConfigConnectionStringsProvider _connectionStringsProvider;
        protected readonly IDatabaseNameBuilder _databaseNameBuilder;

        protected InstanceProvisionerBase(IConfiguration configuration,
            IConfigConnectionStringsProvider connectionStringsProvider, IDatabaseNameBuilder databaseNameBuilder)
        {
            _configuration = configuration;
            _connectionStringsProvider = connectionStringsProvider;
            _databaseNameBuilder = databaseNameBuilder;

            CommandTimeout = int.TryParse(_configuration.GetSection("SandboxAdminSQLCommandTimeout").Value, out int timeout)
                ? timeout
                : 30;

            ConnectionString = _connectionStringsProvider.GetConnectionString("EdFi_Master");
        }

        protected int CommandTimeout { get; }

        protected string ConnectionString { get; }

        public string[] GetInstancesDatabases() => GetInstancesDatabasesAsync().GetResultSafely();

        public void AddDbInstance(string instanceName, DbInstanceType instanceType)
            => AddDbInstanceAsync(instanceName, instanceType).WaitSafely();

        public void AddDbInstance(string instanceName, DbInstanceType instanceType, bool useSuffix)
            => AddDbInstanceAsync(instanceName, instanceType, useSuffix).WaitSafely();

        public void DeleteDbInstances(params string[] deletedClientKeys) => DeleteDbInstancesAsync(deletedClientKeys).WaitSafely();

        public void RenameDbInstances(string oldName, string newName) => RenameDbInstancesAsync(oldName, newName).WaitSafely();

        public InstanceStatus GetDbInstanceStatus(string clientKey) => GetDbInstanceStatusAsync(clientKey).GetResultSafely();

        public void ResetDemoDbInstance() => ResetDemoDbInstanceAsync().WaitSafely();

        public async Task AddDbInstanceAsync(string instanceName, DbInstanceType instanceType)
        {
            await AddDbInstanceAsync(instanceName, instanceType, false);
        }
        public async Task AddDbInstanceAsync(string instanceName, DbInstanceType instanceType, bool useSuffix)
        {
            var newInstanceName = _databaseNameBuilder.SandboxNameForKey(instanceName);
            await DeleteDbInstancesAsync(newInstanceName).ConfigureAwait(false);

            switch (instanceType)
            {
                case DbInstanceType.Minimal:
                    await CopyDbInstanceAsync(
                            _databaseNameBuilder.MinimalDatabase,
                            newInstanceName)
                        .ConfigureAwait(false);

                    break;
                case DbInstanceType.Sample:
                    await CopyDbInstanceAsync(
                            _databaseNameBuilder.SampleDatabase,
                            newInstanceName)
                        .ConfigureAwait(false);

                    break;
                default:
                    throw new Exception("Unhandled InstanceType provided");
            }
        }

        public abstract Task DeleteDbInstancesAsync(params string[] deletedClientKeys);

        public abstract Task<InstanceStatus> GetDbInstanceStatusAsync(string clientKey);

        public async Task ResetDemoDbInstanceAsync()
        {
            var tmpName = Guid.NewGuid().ToString("N");
            await CopyDbInstanceAsync(_databaseNameBuilder.SampleDatabase, tmpName).ConfigureAwait(false);
            await DeleteDbInstancesAsync(_databaseNameBuilder.DemoSandboxDatabase).ConfigureAwait(false);
            await RenameDbInstancesAsync(tmpName, _databaseNameBuilder.DemoSandboxDatabase).ConfigureAwait(false);
        }

        public abstract Task<string[]> GetInstancesDatabasesAsync();

        public abstract Task RenameDbInstancesAsync(string oldName, string newName);

        public abstract Task CopyDbInstanceAsync(string originalDatabaseName, string newDatabaseName);

        protected abstract DbConnection CreateConnection();

        public abstract Task<bool> CheckDatabaseExists(string instanceName);
    }
}
