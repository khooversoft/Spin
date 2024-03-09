using System.Text.Json.Serialization;

namespace Toolbox.Graph;

[JsonDerivedType(typeof(GraphNode), typeDiscriminator: "GraphNode")]
[JsonDerivedType(typeof(GraphEdge), typeDiscriminator: "GraphEdge")]
public interface IGraphCommon
{
}
