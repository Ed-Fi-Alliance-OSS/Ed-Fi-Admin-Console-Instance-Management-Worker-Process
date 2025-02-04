// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.InstanceMgrWorker.Core
{
    public interface IAdminApiSettings
    {
        string ApiUrl { get; set; }
        string AdminConsoleInstancesURI { get; set; }
        string AdminConsoleHealthCheckURI { get; set; }
        string AccessTokenUrl { get; set; }
    }

    public sealed class AdminApiSettings : IAdminApiSettings
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string AdminConsoleInstancesURI { get; set; } = string.Empty;
        public string AdminConsoleHealthCheckURI { get; set; } = string.Empty;
        public string AccessTokenUrl { get; set; } = string.Empty;
    }
}
