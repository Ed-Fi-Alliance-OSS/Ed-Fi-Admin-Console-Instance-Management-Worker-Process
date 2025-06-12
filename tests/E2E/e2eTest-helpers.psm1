# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

<#
.DESCRIPTION
Reads values from the .env file.
#>
function Read-EnvVariables {
    $envFilePath = "$PSScriptRoot/.env"
    $envFileContent = Get-Content -Path $envFilePath

    foreach ($line in $envFileContent) {
        if ($line -match "^\s*([^#][^=]+)=(.*)$") {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [System.Environment]::SetEnvironmentVariable($name, $value)
        }
    }
}

<#
.DESCRIPTION
Creates a new user on Admin API
#>
function Register-AdminApiClient {
    $headers = @{
        "Content-Type" = "application/x-www-form-urlencoded"
        "tenant" = $env:DEFAULTTENANT
    }

    $body = @{
        "ClientId" = $env:clientId
        "ClientSecret" = $env:clientSecret
        "DisplayName" = $env:clientId
    }

    try {
        $response = Invoke-RestMethod -SkipCertificateCheck -Uri "$env:ADMIN_API/connect/register" -Method Post -Headers $headers -Body $body -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $RegisterUrl. Error: $_" -ErrorAction Stop
    }
}

<#
.DESCRIPTION
Returns token from Admin API
#>
function Get-Token {
    $headers = @{
        "Content-Type"  = "application/x-www-form-urlencoded"
        "tenant" = "$env:DEFAULTTENANT"
    }

    $body = @{
        "client_id" = $env:clientId
        "client_secret" = $env:clientSecret
        "grant_type" = "client_credentials"
        "scope" = "edfi_admin_api/full_access"
    }

    try {
        $response = Invoke-RestMethod -SkipCertificateCheck -Uri "$env:ADMIN_API/connect/token" -Method Post -Headers $headers -Body $body -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $TokenUrl. Error: $_" -ErrorAction Stop
    }
}

<#
.DESCRIPTION
Makes an authenticated request to Admin Api
#>
function Invoke-AdminApi { 
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $access_token,

        [Parameter(Mandatory = $true)]
        [string]
        $endpoint,

        [Parameter(Mandatory = $false)]
        [bool]
        $adminConsoleApi = $false,
        
        [Parameter(Mandatory = $false)]
        [string]
        $method = "POST",
        
        [Parameter(Mandatory = $false)]
        [string]
        $filePath    
    )

    $headers = @{
        "Authorization" = "Bearer $access_token"
        "Content-Type"  = "application/json"
        "tenant" = $env:DEFAULTTENANT
    }

    $uri = "$env:ADMIN_API/v2/$endpoint"

    if ($adminConsoleApi -eq $true) {
        

        $uri = "$env:ADMIN_API/adminconsole/$endpoint"
    }

    try {
        if ($method -eq "POST") {
            $response = Invoke-RestMethod -SkipCertificateCheck -Uri $uri -Method Post -Headers $headers -InFile $filePath -StatusCodeVariable statusCode -ResponseHeadersVariable responseHeaders
        }
        else{
            $response = Invoke-RestMethod -SkipCertificateCheck -Uri $uri -Method Get -Headers $headers -StatusCodeVariable statusCode
        }

        return @{
            Body        = $response
            StatusCode  = $statusCode
            ResponseHeaders = $responseHeaders
        }
    }
    catch {
        Write-Error "Failed to send request to $env:ADMIN_API. Error: $_" -ErrorAction Stop
    }
}

Export-ModuleMember -Function Read-EnvVariables, Register-AdminApiClient, Get-Token, Invoke-AdminApi