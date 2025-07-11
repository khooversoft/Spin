using System.Collections.Immutable;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct GraphQueryResult_Surrogate
{
    [Id(0)] public string TransactionId;
    [Id(1)] public Option Option;
    [Id(2)] public QueryResult[]? Items;
}


[RegisterConverter]
public sealed class GraphQueryResult_SurrogateConverter : IConverter<QueryBatchResult, GraphQueryResult_Surrogate>
{
    public QueryBatchResult ConvertFromSurrogate(in GraphQueryResult_Surrogate surrogate) => new QueryBatchResult
    {
        TransactionId = surrogate.TransactionId,
        Option = surrogate.Option,
        Items = surrogate.Items.NotNull().ToImmutableArray(),
    };

    public GraphQueryResult_Surrogate ConvertToSurrogate(in QueryBatchResult subject) => new GraphQueryResult_Surrogate
    {
        TransactionId = subject.TransactionId,
        Option = subject.Option,
        Items = subject.Items.ToArray(),
    };
}