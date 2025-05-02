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
        AdminApiSettings adminApiSettings = new AdminApiSettings();
        adminApiSettings.AccessTokenUrl = "http://www.myserver.com/token";
        adminApiSettings.AdminConsoleTenantsURL = "http://www.myserver.com/adminconsole/tenants";
        adminApiSettings.AdminConsoleInstancesURL = "http://www.myserver.com/adminconsole/instances?status=pending";
        adminApiSettings.AdminConsoleCompleteInstancesURL = "http://www.myserver.com/adminconsole/instances/{0}/completed";
        adminApiSettings.Username = "test-username";
        adminApiSettings.ClientId = "test-clientid";
        adminApiSettings.ClientSecret = "test-clientsecret";
        adminApiSettings.Scope = "test-scope";
        adminApiSettings.GrantType = "client_credentials";
        IOptions<AdminApiSettings> options = Options.Create(adminApiSettings);
        return options;
    }
}
