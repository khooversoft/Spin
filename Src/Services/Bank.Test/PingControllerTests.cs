using Bank.Test.Application;
using FluentAssertions;
using Spin.Common.Client;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Model;
using Xunit;

namespace Bank.Test
{
    public class PingControllerTests
    {
        [Fact]
        public async Task WhenPinged_ShouldReturnOk()
        {
            await testHost(BankName.First);
            await testHost(BankName.Second);

            async Task testHost(BankName bankName)
            {
                PingClient pingClient = TestApplication.GetHost(bankName).GetPingClient();

                (bool ok, PingResponse? response) = await pingClient.Ping();
                ok.Should().BeTrue();
                response.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task WhenReady_ShouldReturnOk()
        {
            await testHost(BankName.First);
            await testHost(BankName.Second);

            async Task testHost(BankName bankName)
            {
                PingClient pingClient = TestApplication.GetHost(BankName.First).GetPingClient();

                (bool ok, PingResponse? response) = await pingClient.Ready();
                ok.Should().BeTrue();
                response.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task WhenAskedForLogs_ShouldReturnData()
        {
            await testHost(BankName.First);
            await testHost(BankName.Second);

            async Task testHost(BankName bankName)
            {
                PingClient pingClient = TestApplication.GetHost(BankName.First).GetPingClient();

                PingLogs? logs = await pingClient.GetLogs();
                logs.Should().NotBeNull();
                logs!.Count.Should().BeGreaterThan(0);
                logs.Messages.Should().NotBeNull();
                logs.Messages!.Count.Should().BeGreaterThan(0);
            }
        }
    }
}