//using System.Collections.Immutable;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public record GraphNodeSearch : IGraphQL
//{
//    public string? Key { get; init; }
//    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
//    public string? Alias { get; init; }
//}


//public static class GraphNodeQueryExtensions
//{
//    public static bool IsMatch(this GraphNodeSearch subject, GraphNode node)
//    {
//        bool isKey = subject.Key == null || node.Key.Like(subject.Key);
//        bool isTag = subject.Tags.Count == 0 || node.Tags.Has(subject.Tags);

//        return isKey && isTag;
//    }
//}