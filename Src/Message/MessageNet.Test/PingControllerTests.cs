﻿using FluentAssertions;
using MessageNet.Test.Application;
using Spin.Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Model;
using Xunit;

namespace MessageNet.Test
{
    public class PingControllerTests
    {
        [Fact]
        public async Task WhenPinged_ShouldReturnOk()
        {
            TestWebsiteHost host = TestApplication.GetHost();

            PingClient pingClient = host.GetPingClient();

            (bool ok, PingResponse? response) = await pingClient.Ping();
            ok.Should().BeTrue();
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenReady_ShouldReturnOk()
        {
            TestWebsiteHost host = TestApplication.GetHost();

            PingClient pingClient = host.GetPingClient();

            (bool ok, PingResponse? response) = await pingClient.Ready();
            ok.Should().BeTrue();
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenAskedForLogs_ShouldReturnData()
        {
            TestWebsiteHost host = TestApplication.GetHost();

            PingClient pingClient = host.GetPingClient();

            PingLogs? logs = await pingClient.GetLogs();
            logs.Should().NotBeNull();
            logs!.Count.Should().BeGreaterThan(0);
            logs.Messages.Should().NotBeNull();
            logs.Messages!.Count.Should().BeGreaterThan(0);
        }
    }
}
