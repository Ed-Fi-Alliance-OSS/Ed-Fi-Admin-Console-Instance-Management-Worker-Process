// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;

public class AdminConsoleInstance
{
    public int TenantId { get; set; } = 0;
    [JsonPropertyName("docId")]
    public int InstanceId { get; set; } = 0;
    public string InstanceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("tenantId")]
    public int OdsInstanceId { get; set; } = 0;
    [JsonPropertyName("document")]
    public JsonElement? Document { get; set; }
    [JsonPropertyName("apiCredentials")]
    public ApiCredentials? ApiCredentials { get; set; }
}
