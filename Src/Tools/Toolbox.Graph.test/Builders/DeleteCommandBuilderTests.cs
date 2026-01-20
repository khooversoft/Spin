using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Builders;

public class DeleteCommandBuilderTests
{
    private GraphLanguageParser BuildLanguageParser()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(service => service.AddSingleton<GraphLanguageParser>())
            .Build();

        return host.Services.GetRequiredService<GraphLanguageParser>();
    }

    [Fact]
    public void DeleteNode()
    {
        var graphQuery = new DeleteCommandBuilder()
            .SetNodeKey("nodeKey1")
            .Build();

        graphQuery.Be("delete node key=nodeKey1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void DeleteNodeIfExist()
    {
        var graphQuery = new DeleteCommandBuilder()
            .SetNodeKey("nodeKey1")
            .SetIfExist()
            .Build();

        graphQuery.Be("delete node ifexist key=nodeKey1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }
}
