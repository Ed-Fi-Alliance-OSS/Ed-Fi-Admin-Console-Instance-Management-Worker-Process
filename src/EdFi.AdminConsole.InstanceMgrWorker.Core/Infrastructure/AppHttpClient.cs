// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Extensions;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace EdFi.AdminConsole.InstanceMgrWorker.Core.Infrastructure
{
    public interface IAppHttpClient
    {
        Task<ApiResponse> SendAsync(string uriString, HttpMethod method, StringContent? content, AuthenticationHeaderValue? authenticationHeaderValue);

        Task<ApiResponse> SendAsync(string uriString, HttpMethod method, FormUrlEncodedContent content, AuthenticationHeaderValue? authenticationHeaderValue);
    }

    public class AppHttpClient(HttpClient httpClient, ILogger logger, IOptions<AppSettings> options) : IAppHttpClient
    {
        private readonly HttpClient _httpClient = httpClient;
        protected readonly ILogger _logger = logger;
        protected readonly AppSettings _options = options.Value;

        public async Task<ApiResponse> SendAsync(string uriString, HttpMethod method, StringContent? content, AuthenticationHeaderValue? authenticationHeaderValue)
        {
            var getByIdDelay = Backoff.ExponentialBackoff(
            TimeSpan.FromMilliseconds(Constants.RetryStartingDelayMilliseconds),
            _options.MaxRetryAttempts);

            int attempts = 0;

            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode.IsPotentiallyTransientFailure())
                .WaitAndRetryAsync(
                    getByIdDelay,
                    (result, ts, retryAttempt, ctx) =>
                    {
                        _logger.LogWarning("Retrying GET for resource '{UriString}'. Failed with status '{StatusCode}'. Retrying... (retry #{RetryAttempt} of {MaxRetryAttempts} with {TotalSeconds:N1}s delay)",
                            uriString, result.Result.StatusCode, retryAttempt, _options.MaxRetryAttempts, ts.TotalSeconds);
                    });

            var response = await retryPolicy.ExecuteAsync(
                async (ctx, ct) =>
                {
                    attempts++;

                    if (attempts > 1)
                    {
                        _logger.LogDebug("GET for resource '{UriString}'. Attempt #{GetByIdAttempts}.",
                            uriString, attempts);
                    }

                    var requestMessage = new HttpRequestMessage(method, uriString)
                    {
                        Content = content
                    };

                    if (authenticationHeaderValue != null)
                    {
                        requestMessage.Headers.Authorization = authenticationHeaderValue;
                    }

                    return await _httpClient.SendAsync(requestMessage, ct);
                },
                [],
                CancellationToken.None);

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var message = $"GET request for '{uriString}' reference failed with status '{response.StatusCode}': {responseContent}";
                _logger.LogWarning(message);
            }

            return new ApiResponse(response.StatusCode, responseContent, response.Headers);
        }

        /// Access Token
        public async Task<ApiResponse> SendAsync(string uriString, HttpMethod method, FormUrlEncodedContent content, AuthenticationHeaderValue? authenticationHeaderValue)
        {
            using var requestMessage = new HttpRequestMessage(method, uriString)
            {
                Content = content
            };

            if (authenticationHeaderValue != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            }

            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse(response.StatusCode, responseContent, response.Headers);
        }
    }
}
