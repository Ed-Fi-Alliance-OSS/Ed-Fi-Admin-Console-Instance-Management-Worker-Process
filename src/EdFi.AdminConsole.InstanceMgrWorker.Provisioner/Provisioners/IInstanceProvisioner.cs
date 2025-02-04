// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners
{
    public interface IInstanceProvisioner
    {
        void AddDbInstance(string instanceName, DbInstanceType instanceType);

        void AddDbInstance(string instanceName, DbInstanceType instanceType, bool useSuffix);

        void DeleteDbInstances(params string[] deletedClientKeys);

        void RenameDbInstances(string oldName, string newName);

        InstanceStatus GetDbInstanceStatus(string clientKey);

        void ResetDemoDbInstance();

        string[] GetInstancesDatabases();

        Task AddDbInstanceAsync(string instanceName, DbInstanceType instanceType);

        Task AddDbInstanceAsync(string instanceName, DbInstanceType instanceType, bool useSuffix);

        Task DeleteDbInstancesAsync(params string[] deletedClientKeys);

        Task RenameDbInstancesAsync(string oldName, string newName);

        Task<InstanceStatus> GetDbInstanceStatusAsync(string clientKey);

        Task ResetDemoDbInstanceAsync();

        Task<string[]> GetInstancesDatabasesAsync();

        Task CopyDbInstanceAsync(string originalDatabaseName, string newDatabaseName);

        Task<bool> CheckDatabaseExists(string instanceName);
    }
}
