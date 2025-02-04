// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.InstanceMgrWorker.Core.Helpers;
using Microsoft.Extensions.Configuration;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core
{
    public interface ICommandArgs
    {
        bool IsMultiTenant { get; }
        string Tenant { get; }
        string ClientId { get; }
        string ClientSecret { get; }
    }

    public class CommandArgs : ICommandArgs
    {
        private readonly IConfiguration _configuration;

        public CommandArgs(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsMultiTenant { get => bool.Parse(_configuration["isMultiTenant"] ?? "false"); }

        public string Tenant { get => _configuration[Constants.TenantHeader] ?? string.Empty; }

        public string ClientId { get => _configuration["clientid"] ?? "test22"; }

        public string ClientSecret { get => _configuration["clientsecret"] ?? "Je}d5TIrwe16ANh'6OGLyxtr(H0h39Fx"; }
    }
}
