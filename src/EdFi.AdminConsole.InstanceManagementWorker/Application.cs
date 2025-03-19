// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners;
using EdFi.AdminConsole.InstanceMgrWorker.Core;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InstanceStatus = EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi.InstanceStatus;

namespace EdFi.AdminConsole.InstanceManagementWorker;

public interface IApplication
{
    Task CreateInstances();
    Task DeleteInstances();
    Task Run();
}

public class Application(
    ILogger logger,
    IAdminApiCaller adminApiCaller,
    IInstanceProvisioner instanceProvisioner,
    IAppSettings appSettings) : IApplication, IHostedService
{
    private readonly ILogger _logger = logger;
    private readonly IAdminApiCaller _adminApiCaller = adminApiCaller;
    private readonly IInstanceProvisioner _instanceProvisioner = instanceProvisioner;
    private readonly IAppSettings _appSettings = appSettings;

    public async Task Run()
    {
        await CreateInstances();
    }

    public async Task CreateInstances()
    {
        _logger.LogInformation("Get tenants on Admin Api.");
        var tenants = await _adminApiCaller.GetTenantsAsync();

        if (!tenants.Any())
            _logger.LogInformation("No tenants returned from Admin Api.");
        else
        {
            foreach (var tenantName in tenants.Select(tenant => tenant.Document.Name))
            {
                var instances = await _adminApiCaller.GetInstancesAsync(tenantName);

                if (instances == null || !instances.Any())
                {
                    _logger.LogInformation("No instances found on Admin Api for tenant {TenantName}", tenantName);
                }
                else
                {
                    foreach (var instance in instances)
                    {
                        var instanceName = instance.InstanceName;

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

                            if (!await _adminApiCaller.CompleteInstanceAsync(instance.Id, instance.TenantName))
                                _logger.LogError("Not able to complete instance.");
                        }
                    }
                    _logger.LogInformation("Process completed.");
                }
            }
        }
    }

    public async Task DeleteInstances()
    {

        var tenants = await _adminApiCaller.GetTenantsAsync();
        var tenantNames = tenants.Select(tenant => tenant.Document.Name).ToList();
        foreach (var tenantName in tenantNames)
        {
            /// Step 1. Get instances data from Admin API - Admin Console extension.
            var instances = await _adminApiCaller.GetInstancesAsync(tenantName, nameof(InstanceStatus.Pending_Delete));

            if (instances == null || !instances.Any())
            {
                _logger.LogInformation("No instances found on Admin Api with status == PENDING_DELETE. For Tenant {TenantName}", tenantName);
                continue;
            }

            var instancesNames = instances.Select(instance => instance.InstanceName).ToList();
            foreach (var instanceName in instancesNames)
            {
                try
                {
                    _logger.LogInformation("Deleting instance: {InstanceName}", instanceName);
                    await _instanceProvisioner.DeleteDbInstancesAsync(instanceName);
                    _logger.LogInformation("Instance {InstanceName} deleted successfully.", instanceName);
                    /// TODO: Call POST /adminconsole/instances/{id}/deleted to mark the Instance as DELETED
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete instance: {InstanceName}", instanceName);
                    /// TODO: Call POST /adminconsole/instances/{id}/deleteFailed to mark the Instance as DELETE_FAILED
                }
            }
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
