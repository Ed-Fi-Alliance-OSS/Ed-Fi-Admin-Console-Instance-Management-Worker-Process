// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.InstanceMgrWorker.Core
{
    public interface IAdminApiSettings
    {
        string AdminConsoleTenantsURI { get; set; }
        string AdminConsoleInstancesURI { get; set; }
        string AdminConsoleCompleteInstancesURI { get; set; }
        string AccessTokenUrl { get; set; }
        string Username { get; set; }
        string ClientId { get; set; }
        string Password { get; set; }
    }

    public sealed class AdminApiSettings : IAdminApiSettings
    {
        public string AdminConsoleTenantsURI { get; set; } = string.Empty;
        public string AdminConsoleInstancesURI { get; set; } = string.Empty;
        public string AdminConsoleCompleteInstancesURI { get; set; } = string.Empty;
        public string AccessTokenUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
