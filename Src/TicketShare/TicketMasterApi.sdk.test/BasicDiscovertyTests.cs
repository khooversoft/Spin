//using TicketMasterApi.sdk.Model;
using TicketMasterApi.sdk.test.Application;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Toolbox.Extensions;
using System.Text.Json;

namespace TicketMasterApi.sdk.test;

public class BasicDiscovertyTests
{
    [Fact]
    public async Task TestSearch()
    {
        var testHost = TestClientHostTool.Create();
        TicketMasterClient client = testHost.GetClient();
        var context = testHost.GetContext<BasicDiscovertyTests>();

        var search = new TicketMasterSearch
        {
            PromoterId = "695,690",
            Page = 0,
            Size = 100,
        };

        var result = await client.GetEvents(search, context);
        result.IsOk().Should().BeTrue();
        result.Return().NotNull();
        result.Return().Count.Assert(x => x > 10, _ => "Empty list");    }
}
