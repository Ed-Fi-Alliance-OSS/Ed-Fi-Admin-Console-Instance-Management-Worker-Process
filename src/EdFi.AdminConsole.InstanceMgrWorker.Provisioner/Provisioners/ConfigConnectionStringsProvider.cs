// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Configuration;

namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners
{
    public class ConfigConnectionStringsProvider : IMgrWorkerConfigConnectionStringsProvider
    {
        private string _tenant;
        private readonly IConfiguration _config;

        public ConfigConnectionStringsProvider(IConfiguration config)
        {
            _tenant = string.Empty;
            _config = config;
        }

        public int Count => ConnectionStringProviderByName.Keys.Count;

        public IDictionary<string, string> ConnectionStringProviderByName
        {
            get
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                return _config.GetSection($"Tenants:{_tenant}:ConnectionStrings")
                    .GetChildren()
                    .ToDictionary(k => k.Key, v => v.Value);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }
        }

        public void SetTenant(string tenant) => _tenant = tenant;

        public string GetConnectionString(string name) => ConnectionStringProviderByName[name];
    }
}
