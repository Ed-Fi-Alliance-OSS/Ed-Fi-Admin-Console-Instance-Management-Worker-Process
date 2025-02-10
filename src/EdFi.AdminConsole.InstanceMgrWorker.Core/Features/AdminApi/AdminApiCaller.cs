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
    Task<IEnumerable<AdminConsoleInstance>> GetInstancesAsync();
}

public class AdminApiCaller : IAdminApiCaller
{
    private readonly ILogger _logger;
    private readonly IAdminApiClient _adminApiClient;
    private readonly IAdminApiSettings _adminApiOptions;
    private readonly ICommandArgs _commandArgs;

    public AdminApiCaller(ILogger logger, IAdminApiClient adminApiClient, IOptions<AdminApiSettings> adminApiOptions, ICommandArgs commandArgs)
    {
        _logger = logger;
        _adminApiClient = adminApiClient;
        _adminApiOptions = adminApiOptions.Value;
        _commandArgs = commandArgs;
    }

    public async Task<IEnumerable<AdminConsoleInstance>> GetInstancesAsync()
    {
        if (AdminApiConnectionDataValidator.IsValid(_logger, _adminApiOptions, _commandArgs))
        {
            var response = await _adminApiClient.AdminApiGet("Getting instances from Admin API - Admin Console extension");
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
}
