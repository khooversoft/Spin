using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public enum SchemaType
{
    Node,
    Tags,
    Index,
    Reference,
    DataName,
    Select,
}

public interface ISchemaValue
{
    SchemaType Type { get; init; }
    string? Attribute { get; init; }
}

public interface ISchemaScalar<T> : ISchemaValue
{
    string? GetScalarValue(T subject);
}

public interface ISchemaCollection<T> : ISchemaValue
{
    IReadOnlyList<string> GetCollectionValues(T subject);
}

public record SchemaConstant : ISchemaValue
{
    public SchemaType Type { get; init; }
    public string? Attribute { get; init; }
}

public record SchemaValue<T, TProperty> : ISchemaScalar<T>
{
    public SchemaType Type { get; init; }
    public Func<T, TProperty> GetSourceValue { get; init; } = null!;
    public Func<TProperty, string?> FormatValue { get; init; } = null!;
    public string? Attribute { get; init; }

    public string? GetScalarValue(T subject) => FormatValue(GetSourceValue(subject));
}

public record SchemaTwoValues<T, TProperty> : ISchemaScalar<T>
{
    public SchemaType Type { get; init; }
    public Func<T, TProperty> GetSourceValue1 { get; init; } = null!;
    public Func<T, TProperty> GetSourceValue2 { get; init; } = null!;
    public Func<TProperty, TProperty, string?> FormatValue { get; init; } = null!;
    public string? Attribute { get; init; }
    public string? GetScalarValue(T subject) => FormatValue(GetSourceValue1(subject), GetSourceValue2(subject));
}

public record SchemaValues<T, TProperty> : ISchemaCollection<T>
{
    public SchemaType Type { get; init; }
    public Func<T, IEnumerable<TProperty>> GetSourceValues { get; init; } = null!;
    public Func<TProperty, string?> FormatValue { get; init; } = null!;
    public string? Attribute { get; init; }

    public IReadOnlyList<string> GetCollectionValues(T subject)
    {
        var values = GetSourceValues(subject);
        var list = values.Select(x => FormatValue(x).ToNullIfEmpty()).OfType<string>().ToArray();
        return list;
    }
}

public static class SchemaValueTool
{
    //public static string GetNodeKey<T>(this IReadOnlyList<ISchemaValue> graphValues, T subject) => graphValues.GetCommand(subject, SchemaType.Node, null);
    public static string GetNodeKey<T>(this IReadOnlyList<ISchemaValue> graphValues, T subject) => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.Node)
        .OfType<ISchemaScalar<T>>()
        .Select(x => x.GetScalarValue(subject))
        .OfType<string>()
        .FirstOrDefault()
        .NotEmpty("Schema definition for Node is not found");

    public static string? GetNodeDataName(this IReadOnlyList<ISchemaValue> graphValues) => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.DataName)
        .OfType<SchemaConstant>()
        .Select(x => x.Attribute)
        .Where(x => x.IsNotEmpty())
        .FirstOrDefault();

    public static string GetTags<T>(this IReadOnlyList<ISchemaValue> graphValues, T subject) => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.Tags)
        .OfType<ISchemaScalar<T>>()
        .Select(x => x.GetScalarValue(subject))
        .Join(",");

    public static string GetSelectCommand<T>(this IReadOnlyList<ISchemaValue> graphValues, T subject, string queryName = "default") => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.Select && (x.Attribute == null || x.Attribute.EqualsIgnoreCase(queryName)))
        .OfType<ISchemaScalar<T>>()
        .Select(x => x.GetScalarValue(subject))
        .OfType<string>()
        .FirstOrDefault()
        .NotEmpty($"Schema definition for select queryName={queryName}");
}
