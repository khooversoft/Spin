//using TicketMasterApi.sdk.Model;
using TicketApi.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk.test;

public class EventTests
{
    [Fact]
    public async Task TestSearch()
    {
        var testHost = TestClientHostTool.Create();
        TicketEventClient client = testHost.GetEventClient();
        var context = testHost.GetContext<EventTests>();

        var search = new TicketMasterSearch
        {
            PromoterId = "695",
            Page = 0,
            Size = 10,
        };

        var result = await client.GetEvents(search, context);
        result.IsOk().BeTrue();
        result.Return().NotNull();
        result.Return().Count.Assert(x => x > 10, _ => "Empty list");
    }
}
