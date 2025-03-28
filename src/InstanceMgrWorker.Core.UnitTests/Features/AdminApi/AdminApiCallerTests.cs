// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Features.AdminApi;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Infrastructure;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace InstanceMgrWorker.Core.UnitTests.Features.AdminApi;

public class Given_an_admin_api
{
    [TestFixture]
    public class When_tenants_are_returned_from_api : Given_an_admin_api
    {
        private ILogger<When_tenants_are_returned_from_api> _logger;
        private IAdminApiCaller _adminApiCaller;
        private IAdminApiClient _adminApiClient;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<When_tenants_are_returned_from_api>>();

            _adminApiClient = A.Fake<IAdminApiClient>();

            A.CallTo(() => _adminApiClient.AdminApiGet("http://www.myserver.com/adminconsole/tenants", null))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Tenants));

            _adminApiCaller = new AdminApiCaller(_logger, _adminApiClient, Testing.GetAdminApiSettings());
        }

        [Test]
        public async Task should_return_successfully()
        {
            var tenants = await _adminApiCaller.GetTenantsAsync();

            tenants.Count().ShouldBe(2);

            tenants.First().TenantId.ShouldBe(1);
            tenants.First().Document.EdfiApiDiscoveryUrl.ShouldBe("https://api.ed-fi.org/v7.2/api/tenant1");
            tenants.First().Document.Name.ShouldBe("tenant1");

            tenants.ElementAt(1).TenantId.ShouldBe(2);
            tenants.ElementAt(1).Document.EdfiApiDiscoveryUrl.ShouldBe("https://api.ed-fi.org/v7.2/api/tenant2");
            tenants.ElementAt(1).Document.Name.ShouldBe("tenant2");
        }
    }

    [TestFixture]
    public class When_instances_are_returned_from_api : Given_an_admin_api
    {
        private ILogger<When_instances_are_returned_from_api> _logger;
        private IAdminApiCaller _adminApiCaller;
        private IAdminApiClient _adminApiClient;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<When_instances_are_returned_from_api>>();

            _adminApiClient = A.Fake<IAdminApiClient>();

            A.CallTo(() => _adminApiClient.AdminApiGet(Testing.GetAdminApiSettings().Value.AdminConsoleInstancesURL, null))
                .Returns(new ApiResponse(HttpStatusCode.OK, Testing.Instances));

            _adminApiCaller = new AdminApiCaller(_logger, _adminApiClient, Testing.GetAdminApiSettings());
        }

        [Test]
        public async Task should_return_successfully()
        {
            var instances = await _adminApiCaller.GetInstancesAsync(null);

            instances.Count().ShouldBe(2);

            instances.First().TenantId.ShouldBe(1);
            instances.First().TenantName.ShouldBe("tenant1");
            instances.First().Id.ShouldBe(1);
            instances.First().OdsInstanceId.ShouldBe(1);
            instances.First().InstanceName.ShouldBe("instance 1");
            instances.First().ClientId.ShouldBe("one client");
            instances.First().ClientSecret.ShouldBe("one secret");
            instances.First().ResourceUrl.ShouldBe("http://www.myserver.com/data/v3");
            instances.First().OauthUrl.ShouldBe("http://www.myserver.com/connect/token");
            instances.First().Status.ShouldBe("Pending");

            instances.ElementAt(1).TenantId.ShouldBe(2);
            instances.ElementAt(1).TenantName.ShouldBe("tenant2");
            instances.ElementAt(1).Id.ShouldBe(2);
            instances.ElementAt(1).OdsInstanceId.ShouldBe(2);
            instances.ElementAt(1).InstanceName.ShouldBe("instance 2");
            instances.ElementAt(1).ClientId.ShouldBe("another client");
            instances.ElementAt(1).ClientSecret.ShouldBe("another secret");
            instances.ElementAt(1).ResourceUrl.ShouldBe("http://www.myserver.com/data/v3");
            instances.ElementAt(1).OauthUrl.ShouldBe("http://www.myserver.com/connect/token");
            instances.First().Status.ShouldBe("Pending");
        }
    }

    [TestFixture]
    public class When_instances_are_completed : Given_an_admin_api
    {
        private ILogger<When_instances_are_returned_from_api> _logger;
        private IAdminApiCaller _adminApiCaller;
        private IAdminApiClient _adminApiClient;
        private int instanceId = 1;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<When_instances_are_returned_from_api>>();

            _adminApiClient = A.Fake<IAdminApiClient>();

            A.CallTo(() => _adminApiClient.AdminApiPost(string.Format(Testing.GetAdminApiSettings().Value.AdminConsoleCompleteInstancesURL, instanceId), null))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty));

            _adminApiCaller = new AdminApiCaller(_logger, _adminApiClient, Testing.GetAdminApiSettings());
        }

        [Test]
        public async Task should_return_successfully()
        {
            var result = await _adminApiCaller.CompleteInstanceAsync(1, null);

            result.ShouldBe(true);
        }
    }
}
