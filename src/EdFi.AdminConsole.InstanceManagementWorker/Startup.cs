// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Admin.DataAccess.Utils;
using EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners;
using EdFi.AdminConsole.InstanceMgrWorker.Core;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Infrastructure;
using EdFi.Ods.Common.Configuration;
using EdFi.Ods.Common.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IHostedService = Microsoft.Extensions.Hosting.IHostedService;

namespace EdFi.AdminConsole.InstanceManagementWorker
{
    public static class Startup
    {
        public static void ConfigureAppConfiguration(string[] args, IConfigurationBuilder config)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "";
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("EdFi:AdminConsole:")
                .AddCommandLine(args, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["--apiUrl"] = "AdminApiSettings:ApiUrl",
                    ["--masterDb"] = "ConnectionStrings:EdFi_Master",
                    ["--edfiOdsDb"] = "ConnectionStrings:EdFi_Ods",
                    ["--databaseEngine"] = "AppSettings:DatabaseEngine",
                    ["--ignoresCertificateErrors"] = "AppSettings:IgnoresCertificateErrors",
                    ["--overrideExistingDatabase"] = "AppSettings:OverrideExistingDatabase",
                    ["--key"] = "AdminApiSettings:ClientId",
                    ["--secret"] = "AdminApiSettings:ClientSecret",
                });
        }

        public static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IConfiguration>(config);
            services.AddOptions();
            services.Configure<AppSettings>(config.GetSection("AppSettings"));
            services.Configure<AdminApiSettings>(config.GetSection("AdminApiSettings"));
            services.AddLogging(configure => configure.AddConsole());
#pragma warning disable CS8603 // Possible null reference return.
            services.AddSingleton<ILogger>(sp => sp.GetService<ILogger<Application>>());
#pragma warning restore CS8603 // Possible null reference return.
            services.AddTransient<IAppSettings, AppSettings>();
            services.AddTransient<IAdminApiCaller, AdminApiCaller>();
            services.AddTransient<IConfigConnectionStringsProvider, ConfigConnectionStringsProvider>();
            services.AddTransient<IDatabaseEngineProvider, InstanceDatabaseEngineProvider>();
            services.AddTransient<IDbConnectionStringBuilderAdapterFactory, DbConnectionStringBuilderAdapterFactory>();
            services.AddTransient<IDbConnectionStringBuilderAdapter, NpgsqlConnectionStringBuilderAdapter>();
            services.AddTransient<IDatabaseNameBuilder, InstanceDatabaseNameBuilder>();
            services.AddTransient<IInstanceProvisioner, PostgresInstanceProvisioner>();
            services.AddSingleton<ICommandArgs, CommandArgs>();
            services.AddTransient<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>();
            services.AddTransient<IAdminApiClient, AdminApiClient>();
            services.AddTransient<IAdminApiCaller, AdminApiCaller>();
            services.AddTransient<IApplication, Application>();
            services.AddTransient<IHostedService, Application>();
            services
            .AddHttpClient<IAppHttpClient, AppHttpClient>(
                "AppHttpClient",
                x =>
                {
                    x.Timeout = TimeSpan.FromSeconds(500);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                if (
                    config?.GetSection("AppSettings")?["IgnoresCertificateErrors"]?.ToLower() == "true"
                )
                {
                    return IgnoresCertificateErrorsHandler();
                }
                return handler;
            });
            services.AddTransient<AdminApiClient>();
        }
        private static HttpClientHandler IgnoresCertificateErrorsHandler()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
#pragma warning disable S4830 // Server certificates should be verified during SSL/TLS connections
            handler.ServerCertificateCustomValidationCallback = (
                httpRequestMessage,
                cert,
                cetChain,
                policyErrors
            ) =>
            {
                return true;
            };
#pragma warning restore S4830

            return handler;
        }
    }
}
