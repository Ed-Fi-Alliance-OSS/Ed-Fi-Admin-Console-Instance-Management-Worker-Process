// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners
{
    public class ConfigConnectionStringsProvider : IConfigConnectionStringsProvider
    {
        public ConfigConnectionStringsProvider(IConfiguration config)
        {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            ConnectionStringProviderByName = config.GetSection("ConnectionStrings")
                .GetChildren()
                .ToDictionary(k => k.Key, v => v.Value);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        public int Count
        {
            get => ConnectionStringProviderByName.Keys.Count;
        }

        public IDictionary<string, string> ConnectionStringProviderByName { get; }

        public string GetConnectionString(string name) => ConnectionStringProviderByName[name];
    }
}
