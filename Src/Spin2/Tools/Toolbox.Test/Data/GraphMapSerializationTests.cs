using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data;

public class GraphMapSerializationTests
{
    [Fact]
    public void EmptyMap()
    {
        var map = new GraphMap<string, GraphNode<string>, GraphEdge<string>>();

        var json = map.ToJson();

        var mapResult = json.ToObject<GraphMap<string, IGraphNode<string>, IGraphEdge<string>>>();
        mapResult.Should().NotBeNull();
    }
}
