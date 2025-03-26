// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EdFi.AdminConsole.InstanceManagementWorker
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        Startup.ConfigureAppConfiguration(args, config);
                        config.AddEnvironmentVariables(prefix: "EdFi_AdminConsole_");
                    })
                    .ConfigureServices((context, services) =>
                    {
                        Startup.ConfigureServices(services, context.Configuration);
                    })
                    .Build();
            var service = host.Services.GetRequiredService<IApplication>();

            await service.CreateInstances();
            await service.DeleteInstances();
        }
    }
}
