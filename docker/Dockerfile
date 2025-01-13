# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

# Image based on .NET SDK to compile and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0.401-alpine3.20@sha256:658c93223111638f9bb54746679e554b2cf0453d8fb7b9fed32c3c0726c210fe AS build
WORKDIR /source

# Copy source code and compile the application
COPY src/EdFi.AdminConsole.InstanceManagement/. ./EdFi.AdminConsole.InstanceManagement/
WORKDIR /source/EdFi.AdminConsole.InstanceManagement

# Restore dependencies, Then build and publish release
RUN dotnet restore &&\
    dotnet publish -c Release -o /app

# .NET Runtime image to execute the application
FROM mcr.microsoft.com/dotnet/runtime:8.0.11-alpine3.20-amd64@sha256:6970cb2d1f8ba6e87d49cfb35687106f0b80c874171978b60d8962daaa8e097f AS runtime
WORKDIR /app
COPY --from=build /app .

# Execute the app
ENTRYPOINT ["dotnet", "EdFi.AdminConsole.InstanceManagement.dll"]