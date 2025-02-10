// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners;
using EdFi.AdminConsole.InstanceMgrWorker.Core;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.InstanceManagementWorker;

public interface IApplication
{
    Task CreateInstances();
    Task Run();
}

public class Application : IApplication, IHostedService
{
    private readonly ILogger _logger;
    private readonly IAdminApiCaller _adminApiCaller;
    private readonly IInstanceProvisioner _instanceProvisioner;
    private readonly IAppSettings _appSettings;

    public Application(
        ILogger logger,
        IAdminApiCaller adminApiCaller,
        IInstanceProvisioner instanceProvisioner,
        IAppSettings appSettings)
    {
        _logger = logger;
        _adminApiCaller = adminApiCaller;
        _instanceProvisioner = instanceProvisioner;
        _appSettings = appSettings;
    }

    public async Task Run()
    {
        /// Step 1. Get instances data from Admin API - Admin Console extension.
        var instances = await _adminApiCaller.GetInstancesAsync();

        if (instances == null || !instances.Any())
        {
            _logger.LogInformation("No instances found on Admin Api.");
        }
        else
        {
            foreach (var instance in instances)
            {
                /// Step 2. For each instance, Get the HealthCheck data from ODS API
                _logger.LogInformation("Processing instance with name: {InstanceName}", instance.InstanceId.ToString() ?? "<No Name>");
            }

            _logger.LogInformation("Process completed.");
        }
    }

    public async Task CreateInstances()
    {
        /// Step 1. Get instances data from Admin API - Admin Console extension.
        /// TODO: Get instances with status != Completed or Error
        var instances = await _adminApiCaller.GetInstancesAsync();

        if (instances == null || !instances.Any())
        {
            _logger.LogInformation("No instances found on Admin Api.");
        }
        else
        {
            var instancesNames = instances.Select(instance => instance.InstanceName).ToList();
            foreach (var instanceName in instancesNames)
            {
                if (!string.IsNullOrWhiteSpace(instanceName))
                {
                    // Checks if the instance exists or it is a new instance
                    if (!_appSettings.OverrideExistingDatabase
                        && await _instanceProvisioner.CheckDatabaseExists(instanceName))
                    {
                        // TODO: Change status: to Completed or Other because the database already exists
                        _logger.LogInformation("Processing instance with name: {InstanceName} already exists. Skipping processing", instanceName ?? "<No Name>");
                        continue;
                    }
                    _logger.LogInformation("Processing instance with name: {InstanceName}", instanceName);
                    await _instanceProvisioner.AddDbInstanceAsync(instanceName, DbInstanceType.Minimal);
                    // TODO: if the process is successful change the status to Completed
                }
            }
            _logger.LogInformation("Process completed.");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Run();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
