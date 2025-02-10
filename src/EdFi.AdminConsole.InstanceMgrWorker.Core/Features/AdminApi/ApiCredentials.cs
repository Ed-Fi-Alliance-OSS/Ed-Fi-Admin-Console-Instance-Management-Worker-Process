// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Text.Json.Serialization;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi
{
    public class ApiCredentials
    {
        [JsonPropertyName("ClientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("Secret")]
        public string? Secret { get; set; }
    }
}
