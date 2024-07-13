using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

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

public interface ISchemaValue<T>
{
    SchemaType Type { get; init; }
    string? GetResolvedValue(T subject);
    public string? Attribute { get; init; }
}

public class SchemaValue<T, TProperty> : ISchemaValue<T>
{
    public SchemaType Type { get; init; }
    public IReadOnlyList<Func<T, TProperty>> GetSourceValues { get; init; } = null!;
    public Func<IReadOnlyList<TProperty>, string?> FormatValue { get; init; } = null!;
    public string? GetResolvedValue(T subject) => FormatValue(GetSourceValues.Select(x => x(subject)).ToArray());
    public string? Attribute { get; init; }

    public static IValidator<SchemaValue<T, TProperty>> Validator { get; } = new Validator<SchemaValue<T, TProperty>>()
        .RuleFor(x => x.Type).ValidEnum()
        .RuleFor(x => x.GetSourceValues).NotNull()
        .RuleFor(x => x.FormatValue).NotNull()
        .Build();
}

public static class SchemaValueExtensions
{
    public static Option Validate<T, TProperty>(this SchemaValue<T, TProperty> subject) => SchemaValue<T, TProperty>.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate<T, TProperty>(this SchemaValue<T, TProperty> subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static string GetNodeKey<T>(this IReadOnlyList<ISchemaValue<T>> graphValues, T subject) => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.Node)
        .Select(x => x.GetResolvedValue(subject))
        .FirstOrDefault()
        .Assert(x => x != null, "node key not found")!;

    public static string? GetNodeDataName<T>(this IReadOnlyList<ISchemaValue<T>> graphValues, T subject) => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.DataName)
        .Select(x => x.Attribute)
        .FirstOrDefault();

    public static string GetTags<T>(this IReadOnlyList<ISchemaValue<T>> graphValues, T subject) => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.Tags)
        .Select(x => x.GetResolvedValue(subject))
        .Join(",");

    public static string GetSelectCommand<T>(this IReadOnlyList<ISchemaValue<T>> graphValues, T subject, string queryName = "default") => graphValues.NotNull()
        .Where(x => x.Type == SchemaType.Select && x.Attribute == queryName)
        .Select(x => x.GetResolvedValue(subject))
        .OfType<string>()
        .FirstOrDefault()
        .NotEmpty($"Select command not found for query name: {queryName}");
}
