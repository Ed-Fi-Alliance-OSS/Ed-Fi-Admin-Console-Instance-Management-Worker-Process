// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;

public interface IAdminApiClient
{
    Task<ApiResponse> AdminApiGet(string url, string? tenant);

    Task<ApiResponse> AdminApiPost(string url, string? tenant);
}

public class AdminApiClient : IAdminApiClient
{
    private readonly IAppHttpClient _appHttpClient;
    protected readonly ILogger _logger;
    private readonly IAdminApiSettings _adminApiOptions;
    private string _accessToken;

    public AdminApiClient(
        IAppHttpClient appHttpClient,
        ILogger logger,
        IOptions<AdminApiSettings> adminApiOptions
    )
    {
        _appHttpClient = appHttpClient;
        _logger = logger;
        _adminApiOptions = adminApiOptions.Value;
        _accessToken = string.Empty;
    }

    public async Task<ApiResponse> AdminApiGet(string url, string? tenant)
    {
        ApiResponse response = new ApiResponse(HttpStatusCode.InternalServerError, "Unknown error.");
        await GetAccessToken();

        if (!string.IsNullOrEmpty(_accessToken))
        {
            const int RetryAttempts = 3;
            var currentAttempt = 0;

            StringContent? content = null;
            if (!string.IsNullOrEmpty(tenant))
            {
                content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                content.Headers.Add("tenant", tenant);
            }

            while (RetryAttempts > currentAttempt)
            {
                response = await _appHttpClient.SendAsync(url,
                    HttpMethod.Get,
                    content,
                    new AuthenticationHeaderValue("bearer", _accessToken)
                );

                currentAttempt++;

                if (response.StatusCode == HttpStatusCode.OK)
                    break;
            }
        }

        return response;
    }

    public async Task<ApiResponse> AdminApiPost(string url, string? tenant)
    {
        ApiResponse response = new ApiResponse(HttpStatusCode.InternalServerError, "Unknown error.");
        await GetAccessToken();

        const int RetryAttempts = 3;
        var currentAttempt = 0;

        StringContent? content = null;
        if (!string.IsNullOrEmpty(tenant))
        {
            content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            content.Headers.Add("tenant", tenant);
        }

        while (RetryAttempts > currentAttempt)
        {
            response = await _appHttpClient.SendAsync(
                url,
                HttpMethod.Post,
                content,
                new AuthenticationHeaderValue("bearer", _accessToken)
            );

            currentAttempt++;

            if (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK or HttpStatusCode.NoContent)
                break;
        }

        return response;
    }

    protected async Task GetAccessToken()
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("username", _adminApiOptions.Username),
                    new KeyValuePair<string, string>("client_id", _adminApiOptions.ClientId),
                    new KeyValuePair<string, string>("password", _adminApiOptions.Password),
                    new KeyValuePair<string, string>("grant_type", "password")
                }
            );

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var apiResponse = await _appHttpClient.SendAsync(
                _adminApiOptions.AccessTokenUrl,
                HttpMethod.Post,
                content,
                null
            );

            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonToken = JToken.Parse(apiResponse.Content);
                _accessToken = jsonToken["access_token"].ToString();
            }
            else
            {
                _logger.LogError("Not able to get Admin Api Access Token. Status Code: {0}", nameof(apiResponse.StatusCode));
            }
        }
    }
}
