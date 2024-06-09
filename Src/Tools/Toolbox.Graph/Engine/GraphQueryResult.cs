using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public enum CommandType
{
    None,
    AddNode,
    AddEdge,
    UpdateEdge,
    UpdateNode,
    DeleteEdge,
    DeleteNode,
    Select,
}


public record GraphQueryResult
{
    public GraphQueryResult() { }
    public GraphQueryResult(CommandType commandType, Option status) => (CommandType, Status) = (commandType, status);

    public Option Status { get; init; }
    public CommandType? CommandType { get; init; }
    public bool IsMutating => CommandType switch
    {
        Graph.CommandType.AddNode => true,
        Graph.CommandType.AddEdge => true,
        Graph.CommandType.UpdateEdge => true,
        Graph.CommandType.UpdateNode => true,
        Graph.CommandType.DeleteEdge => true,
        Graph.CommandType.DeleteNode => true,
        _ => false,
    };
    public override string ToString() => $"{Status}, {nameof(CommandType)}={CommandType}";

    public ImmutableArray<IGraphCommon> Items { get; init; } = ImmutableArray<IGraphCommon>.Empty;
    public ImmutableDictionary<string, DataETag> ReturnNames { get; init; } = ImmutableDictionary<string, DataETag>.Empty;
    public ImmutableDictionary<string, ImmutableArray<IGraphCommon>> Alias { get; init; } = ImmutableDictionary<string, ImmutableArray<IGraphCommon>>.Empty;
}

public record GraphQueryResults
{
    public ImmutableArray<GraphQueryResult> Items { get; init; } = ImmutableArray<GraphQueryResult>.Empty;
    public bool IsMutating => Items.Any(x => x.IsMutating);
}


public static class GraphQueryResultExtensions
{
    public static ImmutableArray<GraphEdge> Edges(this GraphQueryResult subject) => subject.NotNull().Items.OfType<GraphEdge>().ToImmutableArray();
    public static ImmutableArray<GraphNode> Nodes(this GraphQueryResult subject) => subject.NotNull().Items.OfType<GraphNode>().ToImmutableArray();

    public static ImmutableArray<GraphEdge> AliasEdge(this GraphQueryResult subject, string key) => subject.NotNull().Alias[key].OfType<GraphEdge>().ToImmutableArray();
    public static ImmutableArray<GraphNode> AliasNode(this GraphQueryResult subject, string key) => subject.NotNull().Alias[key].OfType<GraphNode>().ToImmutableArray();

    public static Option<T> ReturnNameToObject<T>(this ImmutableDictionary<string, DataETag> subject, string returnName)
    {
        subject.NotNull();
        if (!subject.TryGetValue(returnName, out var dataETag)) return (StatusCode.NotFound, $"returnName={returnName} not found in 'ReturnNames'");

        var entity = dataETag.ToObject<T>();
        if (entity == null) return (StatusCode.Conflict, $"returnName={returnName} cannot be deserialized");

        return entity;
    }

    public static bool HasScalarResult(this GraphQueryResults subject) => subject.NotNull().Items.Length == 1 && subject.Items.First().Items.Length == 1;

    public static IReadOnlyList<T> Get<T>(this GraphQueryResults subject) => subject.NotNull()
        .Items.First()
        .Items.OfType<T>()
        .ToArray();
}