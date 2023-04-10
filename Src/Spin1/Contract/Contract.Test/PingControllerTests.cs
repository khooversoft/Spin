using Contract.Test.Application;
using FluentAssertions;
using Spin.Common.Client;
using System.Threading.Tasks;
using Toolbox.Model;
using Xunit;

namespace Contract.Test
{
    public class PingControllerTests
    {
        [Fact]
        public async Task WhenPinged_ShouldReturnOk()
        {
            PingClient pingClient = TestApplication.GetPingClient();

            (bool ok, PingResponse? response) = await pingClient.Ping();
            ok.Should().BeTrue();
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenReady_ShouldReturnOk()
        {
            PingClient pingClient = TestApplication.GetPingClient();

            (bool ok, PingResponse? response) = await pingClient.Ready();
            ok.Should().BeTrue();
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenAskedForLogs_ShouldReturnData()
        {
            PingClient pingClient = TestApplication.GetPingClient();

            PingLogs? logs = await pingClient.GetLogs();
            logs.Should().NotBeNull();
            logs!.Count.Should().BeGreaterThan(0);
            logs.Messages.Should().NotBeNull();
            logs.Messages!.Count.Should().BeGreaterThan(0);
        }
    }
}