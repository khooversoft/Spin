//using TicketMasterApi.sdk.Model;
using TicketMasterApi.sdk.test.Application;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Toolbox.Extensions;
using System.Text.Json;

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
        result.Return().Count.Assert(x => x > 10, _ => "Empty list");    }
}
