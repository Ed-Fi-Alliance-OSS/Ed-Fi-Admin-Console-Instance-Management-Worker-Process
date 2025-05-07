// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.Common.Database;
using EdFi.Ods.Common.Extensions;

namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners
{
    public class InstanceDatabaseNameBuilder : IMgrWorkerIDatabaseNameBuilder
    {
        private const string TemplatePrefix = "Ods_";
        private const string TemplateMinimalDatabase = TemplatePrefix + "Minimal_Template";

        private string? _tenant { get; set; }
        private readonly Lazy<string> _databaseNameTemplate;

        public InstanceDatabaseNameBuilder(IMgrWorkerConfigConnectionStringsProvider connectionStringsProvider,
            IDbConnectionStringBuilderAdapterFactory connectionStringBuilderFactory)
        {
            _databaseNameTemplate = new Lazy<string>(
                () =>
                {
                    if (!string.IsNullOrEmpty(_tenant))
                        connectionStringsProvider.SetTenant(_tenant);

                    if (!connectionStringsProvider.ConnectionStringProviderByName.ContainsKey("EdFi_Ods"))
                    {
                        return string.Empty;
                    }

                    var connectionStringBuilder = connectionStringBuilderFactory.Get();

                    connectionStringBuilder.ConnectionString = connectionStringsProvider.GetConnectionString("EdFi_Ods");

                    return connectionStringBuilder.DatabaseName;
                });
        }

        public string DemoSandboxDatabase
        {
            get => "EdFi_Ods";
        }

        public string EmptyDatabase => throw new NotImplementedException();

        public string MinimalDatabase
        {
            get => DatabaseName(_tenant, TemplateMinimalDatabase);
        }

        public string SampleDatabase => throw new NotImplementedException();

        public string SandboxNameForKey(string key) => throw new NotImplementedException();

        public string KeyFromSandboxName(string sandboxName) => throw new NotImplementedException();

        public string TemplateSandboxNameForKey(string sandboxKey) => throw new NotImplementedException();

        public string OdsDatabaseName(string? tenant, string databaseName) => DatabaseName(tenant, TemplatePrefix + databaseName);

        private string DatabaseName(string? tenant, string databaseName)
        {
            _tenant = tenant;
            return _databaseNameTemplate.Value.IsFormatString()
                ? string.Format(_databaseNameTemplate.Value, databaseName)
                : _databaseNameTemplate.Value;
        }
    }
}
