//namespace Toolbox.Data;

//public record GraphCommandExceuteResults
//{
//    public GraphMap GraphMap { get; init; } = null!;
//    public IReadOnlyList<GraphQueryResult> Items { get; init; } = Array.Empty<GraphQueryResult>();
//}


//public static class GraphCommandExceuteResultsExtensions
//{
//    public static GraphCommandResults ConvertTo(this GraphCommandExceuteResults subject) => new GraphCommandResults
//    {
//        Items = subject.Items.ToArray(),
//    };
//}