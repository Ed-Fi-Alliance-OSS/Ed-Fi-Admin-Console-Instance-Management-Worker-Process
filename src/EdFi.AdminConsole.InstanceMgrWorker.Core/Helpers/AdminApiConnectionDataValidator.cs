// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core.Helpers;

public static class AdminApiConnectionDataValidator
{
    public static bool IsValid(ILogger logger, IAdminApiSettings adminApiSettings)
    {
        var messages = new List<string>();

        if (string.IsNullOrEmpty(adminApiSettings.AccessTokenUrl))
            messages.Add("AccessTokenUrl is required.");

        if (string.IsNullOrEmpty(adminApiSettings.AdminConsoleTenantsURL))
            messages.Add("AdminConsoleTenantsURL is required.");

        if (string.IsNullOrEmpty(adminApiSettings.AdminConsoleInstancesURL))
            messages.Add("AdminConsoleInstancesURL is required.");

        if (string.IsNullOrEmpty(adminApiSettings.AdminConsoleCompleteInstancesURL))
            messages.Add("AdminConsoleCompleteInstancesURL is required.");

        if (string.IsNullOrEmpty(adminApiSettings.Username))
            messages.Add("ClientId is required.");

        if (string.IsNullOrEmpty(adminApiSettings.ClientId))
            messages.Add("ClientId is required.");

        if (string.IsNullOrEmpty(adminApiSettings.Password))
            messages.Add("ClientSecret is required.");

        if (messages != null && messages.Count > 0)
        {
            string concatenatedMessages = string.Concat(messages);
            logger.LogWarning("The AdminApiSettings section on the App Settings file and/or the App command arguments have not been set properly. {Messages}", concatenatedMessages);
            return false;
        }

        return true;
    }
}
