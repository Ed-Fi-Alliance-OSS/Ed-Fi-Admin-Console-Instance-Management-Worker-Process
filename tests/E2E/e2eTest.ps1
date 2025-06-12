# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

Import-Module -Name "$PSScriptRoot/e2eTest-helpers.psm1" -Force

# Import Pester for testing (but don't auto-discover)
Import-Module Pester -Force

# Pre-Step
Read-EnvVariables

# Global Variables
$adminApiUrl = $env:ADMIN_API
$adminConsoleInstancesUrl = "$env:ADMIN_API/adminconsole/instances"
$AdminConsoleHealthCheckUrl = "$env:ADMIN_API/adminconsole/healthcheck"

[System.Environment]::SetEnvironmentVariable("clientId", "client-$(New-Guid)")
[System.Environment]::SetEnvironmentVariable("clientSecret", $env:CLIENT_SECRET)

if ($env:MULTITENANCY -ne "true") {
  throw "Single tenant not yet supported."
}

# 1. Register client on Admin Api
Write-Host "Register client..."
$response = Register-AdminApiClient
if ($response.StatusCode -ne 200) {
    Write-Error "Not able to register user on Admin Api." -ErrorAction Stop
}

# 2. Get Token from Admin Api
Write-Host "Get token..."
$response = Get-Token -clientId $env:clientId -clientSecret $env:clientSecret
if ($response.StatusCode -ne 200) {
    Write-Error "Not able to get token on Admin Api." -ErrorAction Stop
}

$access_token = $response.Body.access_token

# 3 Create instance on Admin Api
Copy-Item -Path "$PSScriptRoot/payloads/odsInstance.json" -Destination "$PSScriptRoot/payloads/odsInstanceCopy.json"
(Get-Content $PSScriptRoot/payloads/odsInstanceCopy.json).Replace('%Password%', $env:POSTGRES_PASSWORD) | Set-Content $PSScriptRoot/payloads/odsInstanceCopy.json
(Get-Content $PSScriptRoot/payloads/odsInstanceCopy.json).Replace('%Name%', "ods-$(New-Guid)") | Set-Content $PSScriptRoot/payloads/odsInstanceCopy.json

$response = Invoke-AdminApi -access_token $access_token -filePath "$PSScriptRoot/payloads/odsInstanceCopy.json" -endpoint "odsInstances"
if ($response.StatusCode -ne 201) {
    Write-Error "Not able to create ods instance on Admin Api." -ErrorAction Stop
}

$odsInstanceId = $response.ResponseHeaders.location -replace '\D'
Write-Host "OdsInstanceId: $odsInstanceId"

Remove-Item -Path "$PSScriptRoot/payloads/odsInstanceCopy.json"

# 4. Call Ed-Fi Admin Console Instance Management Worker Process
Write-Host "Call Ed-Fi-Admin-Console-Health-Check-Worker-Process..."
docker run --rm edfi.adminconsole.instancemanagementworker dotnet EdFi.AdminConsole.InstanceManagementWorker.dll --isMultiTenant=true --tenant="$env:DEFAULTTENANT" --ClientId="$env:clientId" --ClientSecret="$env:clientSecret"

# 5. Call Admin Api to check if instance is completed
$response = Invoke-AdminApi -access_token $access_token -endpoint "odsInstances/$odsInstanceId" -method "GET" -adminConsoleApi $True
if ($response.StatusCode -ne 200) {
    Write-Error "Not able to get ods instance on Admin Api." -ErrorAction Stop
}

# 6. Run Pester tests to verify the status
Describe "Instance Management Tests" {
    It "Instance status should be 'completed'" {
        $instanceStatus = $response.Body.status
        Write-Host "Current Instance Status: $instanceStatus"
        $instanceStatus | Should -Be "completed"
    }
}

