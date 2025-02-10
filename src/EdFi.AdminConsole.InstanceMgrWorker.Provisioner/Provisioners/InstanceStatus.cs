// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.
namespace EdFi.AdminConsole.InstanceMgrWorker.Configuration.Provisioners
{
    public class InstanceStatus
    {
        public string? Name { get; set; }

        public byte Code { get; set; }

        public string? Description { get; set; }

        public static InstanceStatus ErrorStatus()
        {
            return new InstanceStatus
            {
                Code = byte.MaxValue,
                Description = "ERROR"
            };
        }
    }
}
