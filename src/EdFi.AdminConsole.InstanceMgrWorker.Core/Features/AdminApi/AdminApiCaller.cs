// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.InstanceMgrWorker.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;

public interface IAdminApiCaller
{
    Task<IEnumerable<AdminConsoleTenant>> GetTenantsAsync();

    Task<IEnumerable<AdminConsoleInstance>> GetInstancesAsync(string? tenant);

    Task<bool> CompleteInstanceAsync(int instanceId, string? tenant);
}

public class AdminApiCaller : IAdminApiCaller
{
    private readonly ILogger _logger;
    private readonly IAdminApiClient _adminApiClient;
    private readonly IAdminApiSettings _adminApiOptions;

    public AdminApiCaller(ILogger logger, IAdminApiClient adminApiClient, IOptions<AdminApiSettings> adminApiOptions)
    {
        _logger = logger;
        _adminApiClient = adminApiClient;
        _adminApiOptions = adminApiOptions.Value;
    }

    public async Task<IEnumerable<AdminConsoleTenant>> GetTenantsAsync()
    {
        if (AdminApiConnectionDataValidator.IsValid(_logger, _adminApiOptions))
        {
            var response = await _adminApiClient.AdminApiGet(_adminApiOptions.AdminConsoleTenantsURI, null);
            var tenants = new List<AdminConsoleTenant>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                var tenantsJObject = JsonConvert.DeserializeObject<IEnumerable<JObject>>(response.Content);
                if (tenantsJObject != null)
                {
                    foreach (var jObjectItem in tenantsJObject)
                    {
                        try
                        {
                            var jsonString = jObjectItem.ToString();
                            if (jsonString.StartsWith("{{") && jsonString.EndsWith("}}"))
                            {
                                jsonString = jsonString.Substring(1, jsonString.Length - 2);
                            }
                            var tenant = JsonConvert.DeserializeObject<AdminConsoleTenant>(jsonString);
                            if (tenant != null)
                                tenants.Add(tenant);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Not able to process tenant.");
                        }
                    }
                }
            }
            return tenants;
        }
        else
        {
            _logger.LogError("AdminApi Settings has not been set properly.");
            return new List<AdminConsoleTenant>();
        }
    }

    public async Task<IEnumerable<AdminConsoleInstance>> GetInstancesAsync(string? tenant)
    {
        if (AdminApiConnectionDataValidator.IsValid(_logger, _adminApiOptions))
        {
            var response = await _adminApiClient.AdminApiGet(_adminApiOptions.AdminConsoleInstancesURI, tenant);
            var instances = new List<AdminConsoleInstance>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                var instancesJObject = JsonConvert.DeserializeObject<IEnumerable<JObject>>(response.Content);
                if (instancesJObject != null)
                {
                    foreach (var jObjectItem in instancesJObject)
                    {
                        try
                        {
                            var jsonString = jObjectItem.ToString();
                            if (jsonString.StartsWith("{{") && jsonString.EndsWith("}}"))
                            {
                                jsonString = jsonString.Substring(1, jsonString.Length - 2);
                            }
                            var instance = JsonConvert.DeserializeObject<AdminConsoleInstance>(jsonString);
                            if (instance != null)
                                instances.Add(instance);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Not able to process instance.");
                        }
                    }
                }
            }
            return instances;
        }
        else
        {
            _logger.LogError("AdminApi Settings has not been set properly.");
            return new List<AdminConsoleInstance>();
        }
    }

    public async Task<bool> CompleteInstanceAsync(int instanceId, string? tenant)
    {
        if (AdminApiConnectionDataValidator.IsValid(_logger, _adminApiOptions))
        {
            var response = await _adminApiClient.AdminApiPost(_adminApiOptions.AdminConsoleCompleteInstancesURI.Replace("{InstanceId}", instanceId.ToString()), tenant);

            return (response.StatusCode is System.Net.HttpStatusCode.NoContent or System.Net.HttpStatusCode.OK);
        }
        else
        {
            _logger.LogError("AdminApi Settings has not been set properly.");
            return false;
        }
    }
}
