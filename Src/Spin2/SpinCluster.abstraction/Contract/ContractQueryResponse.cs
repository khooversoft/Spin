using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public record ContractQueryResponse
{
    [Id(0)] public IReadOnlyList<QueryBlockTypeResponse> Items { get; init; } = Array.Empty<QueryBlockTypeResponse>();

    public static IValidator<ContractQueryResponse> Validator { get; } = new Validator<ContractQueryResponse>()
        .RuleForEach(x => x.Items).Validate(QueryBlockTypeResponse.Validator)
        .Build();
}


[GenerateSerializer, Immutable]
public record QueryBlockTypeResponse
{
    [Id(0)] public string BlockType { get; init; } = null!;
    [Id(1)] public IReadOnlyList<DataBlock> DataBlocks { get; init; } = Array.Empty<DataBlock>();

    public static IValidator<QueryBlockTypeResponse> Validator { get; } = new Validator<QueryBlockTypeResponse>()
        .RuleFor(x => x.BlockType).Must(x => x.IsEmpty() || IdPatterns.IsName(x), _ => "Invalid block type")
        .RuleForEach(x => x.DataBlocks).Validate(DataBlock.Validator)
        .Build();
}

public static class ContractQueryResponseExtensions
{
    public static Option Validate(this ContractQueryResponse subject) => ContractQueryResponse.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ContractQueryResponse subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static Option<T> GetSingle<T>(this ContractQueryResponse subject) => subject
        .NotNull()
        .Items.Where(x => typeof(T).GetTypeName() == x.BlockType)
        .Select(x => x.DataBlocks.First())
        .TakeLast(1)
        .Select(x => x.ToObject<T>())
        .FirstOrDefaultOption();

    public static IReadOnlyList<T> GetItems<T>(this ContractQueryResponse subject) => subject
        .NotNull()
        .Items.Where(x => typeof(T).GetTypeName() == x.BlockType)
        .SelectMany(x => x.DataBlocks)
        .Select(x => x.ToObject<T>())
        .ToArray();
}
