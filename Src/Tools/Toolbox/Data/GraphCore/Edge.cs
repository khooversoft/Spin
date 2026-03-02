using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record Edge
{
    public Edge(string fromKey, string toKey, string edgeType, DataETag? payload = null)
    {
        FromKey = fromKey.NotEmpty();
        ToKey = toKey.NotEmpty();
        FromKey.Assert(x => !x.EqualsIgnoreCase(ToKey), "FromKey and ToKey cannot be the same");
        EdgeType = edgeType.NotEmpty();

        Payload = payload;
        EdgeKey = CreateKey(FromKey, toKey, edgeType);
    }

    public string EdgeKey { get; }
    public string FromKey { get; }
    public string ToKey { get; init; }
    public string EdgeType { get; init; }
    public DataETag? Payload { get; init; }

    public static IValidator<Edge> Validator { get; } = new Validator<Edge>()
        .RuleFor(x => x.EdgeKey).NotEmpty()
        .RuleFor(x => x.FromKey).NotEmpty()
        .RuleFor(x => x.ToKey).NotEmpty()
        .RuleFor(x => x.EdgeType).NotEmpty()
        .Build();

    public static string CreateKey(string fromKey, string toKey, string edgeType)
    {
        fromKey.NotEmpty();
        toKey.NotEmpty();
        edgeType.NotEmpty();

        return $"{edgeType}:{fromKey}->{toKey}".ToLowerInvariant();
    }
}


public static class EdgeTool
{
    public static Option Validate(this Edge edge) => Edge.Validator.Validate(edge).ToOptionStatus();
}