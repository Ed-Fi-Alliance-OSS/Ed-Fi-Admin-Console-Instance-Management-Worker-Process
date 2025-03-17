// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core.Infrastructure
{
    public interface IAppHttpClient
    {
        Task<ApiResponse> SendAsync(string uriString, HttpMethod method, StringContent? content, AuthenticationHeaderValue? authenticationHeaderValue);

        Task<ApiResponse> SendAsync(string uriString, HttpMethod method, FormUrlEncodedContent content, AuthenticationHeaderValue? authenticationHeaderValue);
    }

    public class AppHttpClient : IAppHttpClient
    {
        private readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly IOptions<AppSettings> _options;
        private readonly IHttpRequestMessageBuilder _httpRequestMessageBuilder;

        public AppHttpClient(HttpClient httpClient, ILogger logger, IOptions<AppSettings> options, IHttpRequestMessageBuilder httpRequestMessageBuilder)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options;
            _httpRequestMessageBuilder = httpRequestMessageBuilder;
        }

        public async Task<ApiResponse> SendAsync(string uriString, HttpMethod method, StringContent? content, AuthenticationHeaderValue? authenticationHeaderValue)
        {
            var request = _httpRequestMessageBuilder.GetHttpRequestMessage(uriString, method, content);

            if (authenticationHeaderValue != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            }
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse(response.StatusCode, responseContent, response.Headers);
        }

        /// Access Token
        public async Task<ApiResponse> SendAsync(string uriString, HttpMethod method, FormUrlEncodedContent content, AuthenticationHeaderValue? authenticationHeaderValue)
        {
            var request = _httpRequestMessageBuilder.GetHttpRequestMessage(uriString, method, content);

            if (authenticationHeaderValue != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            }

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse(response.StatusCode, responseContent, response.Headers);
        }
    }
}
