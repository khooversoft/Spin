//using Toolbox.Data;
//using Toolbox.Tools.Validation;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Actors.Directory;

//[GenerateSerializer, Immutable]
//public sealed record DirectoryResponse
//{
//    [Id(0)] public IReadOnlyList<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
//    [Id(1)] public IReadOnlyList<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();

//    public static IValidator<DirectoryResponse> Validator { get; } = new Validator<DirectoryResponse>()
//        .RuleForEach(x => x.Nodes).Validate(GraphNode.Validator)
//        .RuleForEach(x => x.Edges).Validate(GraphEdge.Validator)
//        .Build();
//}


//public static class DirectoryResponseExtensions
//{
//    public static Option Validate(this DirectoryResponse subject) => DirectoryResponse.Validator.Validate(subject).ToOptionStatus();

//    public static bool Validate(this DirectoryResponse subject, out Option result)
//    {
//        result = subject.Validate();
//        return result.IsOk();
//    }
//}
