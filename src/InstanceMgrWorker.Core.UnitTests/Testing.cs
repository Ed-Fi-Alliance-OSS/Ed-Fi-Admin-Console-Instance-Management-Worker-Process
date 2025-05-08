// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.InstanceMgrWorker.Core;
using Microsoft.Extensions.Options;

namespace InstanceMgrWorker.Core.UnitTests;
public class Testing
{
    public const string Tenants =
    @"[{
        ""TenantId"": 1,
        ""Document"": {
            ""EdfiApiDiscoveryUrl"": ""https://api.ed-fi.org/v7.2/api/tenant1"",
            ""Name"" : ""tenant1""
        }
      },{
        ""tenantId"": 2,
        ""Document"": {
            ""EdfiApiDiscoveryUrl"": ""https://api.ed-fi.org/v7.2/api/tenant2"",
            ""Name"" : ""tenant2""
        }
    }]";

    public const string Instances =
    @"[{
        ""tenantId"": 1,
        ""TenantName"": ""tenant1"",
        ""Id"": 1,
        ""odsInstanceId"": 1,
        ""instanceName"": ""instance 1"",
        ""resourceUrl"": ""http://www.myserver.com/data/v3"",
        ""oauthUrl"": ""http://www.myserver.com/connect/token"",
        ""clientId"": ""one client"",
        ""clientSecret"": ""one secret"",
        ""status"": ""Pending""
      },{
        ""tenantId"": 2,
        ""TenantName"": ""tenant2"",
        ""Id"": 2,
        ""odsInstanceId"": 2,
        ""instanceName"": ""instance 2"",
        ""resourceUrl"": ""http://www.myserver.com/data/v3"",
        ""oauthUrl"": ""http://www.myserver.com/connect/token"",
        ""clientId"": ""another client"",
        ""clientSecret"": ""another secret"",
        ""status"": ""Pending""
    }]";

    public static IOptions<AdminApiSettings> GetAdminApiSettings()
    {
        AdminApiSettings adminApiSettings = new()
        {
            AccessTokenUrl = "http://www.myserver.com/token",
            AdminConsoleTenantsURL = "http://www.myserver.com/adminconsole/tenants",
            AdminConsoleInstancesURL = "http://www.myserver.com/adminconsole/instances?status=pending",
            AdminConsoleCompleteInstancesURL = "http://www.myserver.com/adminconsole/instances/{0}/completed",
            Username = "test-username",
            ClientId = "test-clientid",
            ClientSecret = "test-clientsecret",
            Scope = "test-scope",
            GrantType = "client_credentials"
        };
        IOptions<AdminApiSettings> options = Options.Create(adminApiSettings);
        return options;
    }

    public static IOptions<AppSettings> GetAppSettings()
    {
        AppSettings apiSettings = new();
        apiSettings.MaxRetryAttempts = 3;
        IOptions<AppSettings> options = Options.Create(apiSettings);
        return options;
    }
}
