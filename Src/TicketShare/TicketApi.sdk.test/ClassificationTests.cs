//using TicketMasterApi.sdk.Model;
using TicketApi.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk.test;

public class ClassificationTests
{
    [Fact]
    public async Task TestSearch()
    {
        var testHost = TestClientHostTool.Create();
        TicketClassificationClient client = testHost.GetClassificationClient();
        var context = testHost.GetContext<ClassificationTests>();

        var result = await client.GetClassifications(context);
        result.IsOk().BeTrue();
        result.Return().NotNull();
        result.Return().Count.Assert(x => x > 10, _ => "Empty list");
    }
}
