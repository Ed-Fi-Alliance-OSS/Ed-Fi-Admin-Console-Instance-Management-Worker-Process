// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Infrastructure;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace InstanceMgrWorker.Core.UnitTests.Features.AdminApi;

public class Given_an_admin_api_http_client
{
    private ILogger<Given_an_admin_api_http_client> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<Given_an_admin_api_http_client>>();
    }

    [TestFixture]
    public class When_instances_are_requested : Given_an_admin_api_http_client
    {
        [Test]
        public async Task should_return_successfully()
        {
            var httpClient = A.Fake<IAppHttpClient>();
            var instancesUrl = Testing.GetAdminApiSettings().Value.AdminConsoleInstancesURI;

            A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
                .Returns(new ApiResponse(HttpStatusCode.OK, "{ \"access_token\": \"123\"}"));

            A.CallTo(() => httpClient.SendAsync(instancesUrl, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

            var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings());

            var InstancesReponse = await adminApiClient.AdminApiGet(instancesUrl, null);

            InstancesReponse.Content.ShouldBeEquivalentTo(Testing.Instances);
        }
    }

    [TestFixture]
    public class When_instances_are_requested_without_token : Given_an_admin_api_http_client
    {
        [Test]
        public async Task should_return_successfully()
        {
            var httpClient = A.Fake<IAppHttpClient>();
            var instancesUrl = Testing.GetAdminApiSettings().Value.AdminConsoleInstancesURI;

            A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
                .Returns(new ApiResponse(HttpStatusCode.InternalServerError, string.Empty));

            A.CallTo(() => httpClient.SendAsync(instancesUrl, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

            var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings());

            var InstancesReponse = await adminApiClient.AdminApiGet(instancesUrl, null);

            InstancesReponse.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.InternalServerError);
        }
    }

    [TestFixture]
    public class When_tenants_are_requested : Given_an_admin_api_http_client
    {
        [Test]
        public async Task should_return_successfully()
        {
            var httpClient = A.Fake<IAppHttpClient>();
            var tenantsUrl = Testing.GetAdminApiSettings().Value.AdminConsoleTenantsURI;

            A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
                .Returns(new ApiResponse(HttpStatusCode.OK, "{ \"access_token\": \"123\"}"));

            A.CallTo(() => httpClient.SendAsync(tenantsUrl, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Tenants));

            var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings());

            var InstancesReponse = await adminApiClient.AdminApiGet(tenantsUrl, null);

            InstancesReponse.Content.ShouldBeEquivalentTo(Testing.Tenants);
        }
    }

    [TestFixture]
    public class When_tenants_are_requested_without_token : Given_an_admin_api_http_client
    {
        [Test]
        public async Task should_return_successfully()
        {
            var httpClient = A.Fake<IAppHttpClient>();
            var tenantsUrl = Testing.GetAdminApiSettings().Value.AdminConsoleTenantsURI;

            A.CallTo(() => httpClient.SendAsync(Testing.GetAdminApiSettings().Value.AccessTokenUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, null))
                .Returns(new ApiResponse(HttpStatusCode.InternalServerError, string.Empty));

            A.CallTo(() => httpClient.SendAsync(tenantsUrl, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Tenants));

            var adminApiClient = new AdminApiClient(httpClient, _logger, Testing.GetAdminApiSettings());

            var InstancesReponse = await adminApiClient.AdminApiGet(tenantsUrl, null);

            InstancesReponse.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.InternalServerError);
        }
    }
}
