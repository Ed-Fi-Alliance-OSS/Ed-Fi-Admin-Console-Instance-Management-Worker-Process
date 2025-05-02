// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.InstanceMgrWorker.Core;
using EdFi.AdminConsole.InstanceMgrWorker.Core.Helpers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace InstanceMgrWorker.Core.UnitTests.Helpers;

public class Given_AdminApiSettings_provided
{
    private AdminApiSettings _adminApiSettings = new AdminApiSettings();
    private ILogger<Given_AdminApiSettings_provided> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<Given_AdminApiSettings_provided>>();

        _adminApiSettings.AccessTokenUrl = "http://www.myserver.com/token";
        _adminApiSettings.AdminConsoleTenantsURL = "http://www.myserver.com/adminconsole/tenants";
        _adminApiSettings.AdminConsoleInstancesURL = "http://www.myserver.com/adminconsole/instances";
        _adminApiSettings.AdminConsoleCompleteInstancesURL = "http://www.myserver.com/adminconsole/complete";
        _adminApiSettings.Username = "test-username";
        _adminApiSettings.ClientId = "test-clientid";
        _adminApiSettings.ClientSecret = "test-clientsecret";
        _adminApiSettings.Scope = "test-scope";
        _adminApiSettings.GrantType = "client_credentials";
    }

    [TestFixture]
    public class When_it_has_all_required_fields : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_valid()
        {
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_AccessTokenUrl : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.AccessTokenUrl = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_AdminConsoleTenantsURL : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.AdminConsoleTenantsURL = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_AdminConsoleInstancesURL : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.AdminConsoleInstancesURL = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_AdminConsoleCompleteInstancesURL : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.AdminConsoleCompleteInstancesURL = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ClientId : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.ClientId = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ClientSecret : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.ClientSecret = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_Scope : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.Scope = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_GrantType : Given_AdminApiSettings_provided
    {
        [Test]
        public void should_be_invalid()
        {
            _adminApiSettings.GrantType = string.Empty;
            AdminApiConnectionDataValidator.IsValid(_logger, _adminApiSettings).ShouldBeFalse();
        }
    }
}
