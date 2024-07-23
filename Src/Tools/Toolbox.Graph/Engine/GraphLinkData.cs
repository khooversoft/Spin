using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GraphLinkData
{
    public string NodeKey { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public string Schema { get; init; } = null!;
    public string FileId { get; init; } = null!;
    public DataETag Data { get; init; }

    public static IValidator<GraphLinkData> Validator { get; } = new Validator<GraphLinkData>()
        .RuleFor(x => x.NodeKey).NotEmpty()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.TypeName).ValidName()
        .RuleFor(x => x.Schema).ValidName()
        .RuleFor(x => x.FileId).Must(x => IdPatterns.IsPath(x), x => $"Invalid File path={x}")
        .RuleFor(x => x.Data).Validate(DataETag.Validator)
        .Build();
}

public static class GraphLinkDataTool
{
    public static Option Validate(this GraphLinkData subject) => GraphLinkData.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphLinkData subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static GraphLinkData ConvertTo(this GraphLink subject, DataETag data)
    {
        subject.NotNull();

        var result = new GraphLinkData
        {
            NodeKey = subject.NodeKey,
            Name = subject.Name,
            TypeName = subject.TypeName,
            Schema = subject.Schema,
            FileId = subject.FileId,
            Data = data,
        };

        return result;
    }
}