//using TicketMasterApi.sdk.Model;
using Microsoft.Extensions.DependencyInjection;
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
        using var testHost = TestClientHostTool.Create();

        TicketMasterClient client = testHost.Services.GetRequiredService<TicketMasterClient>();
        var context = testHost.GetContext<ClassificationTests>();

        var result = await client.GetClassifications(context);
        result.IsOk().BeTrue();
        result.Return().NotNull().Segments.Count.Be(2);
    }
}
