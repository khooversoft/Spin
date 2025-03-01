//using TicketMasterApi.sdk.Model;
using TicketMasterApi.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace TicketMasterApi.sdk.test;

public class ClassificationTests
{
    [Fact]
    public async Task TestSearch()
    {
        var testHost = TestClientHostTool.Create();
        TicketMasterClassificationClient client = testHost.GetClassificationClient();
        var context = testHost.GetContext<ClassificationTests>();

        var result = await client.GetClassifications(context);
        result.IsOk().Should().BeTrue();
        result.Return().NotNull();
        result.Return().Count.Assert(x => x > 10, _ => "Empty list");
    }
}
