using System.Diagnostics.CodeAnalysis;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public record DataObject
{
    public required string Key { get; init; } = null!;
    public required string TypeName { get; init; } = null!;
    public required IReadOnlyList<KeyValuePair<string, string>> Values { get; init; } = null!;
}


public static class DataObjectValidator
{
    public static Validator<DataObject> Validator { get; } = new Validator<DataObject>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.TypeName).NotEmpty()
        .RuleFor(x => x.Values).NotNull()
        .RuleForEach(x => x.Values).NotNull()
        .Build();

    public static ValidatorResult Validate(this DataObject subject, ScopeContext context) => Validator.Validate(subject);

    public static DataObject ToDataObject<T>(this T value, string? key = null) where T : class
    {
        IReadOnlyList<KeyValuePair<string, string>> values = value.GetConfigurationValues();

        return new DataObject
        {
            Key = key ?? typeof(T).GetTypeName(),
            TypeName = typeof(T).GetTypeName(),
            Values = values.ToArray(),
        };
    }

    public static T ToObject<T>(this DataObject dataObject) where T : new()
    {
        return dataObject switch
        {
            var v when typeof(T) == typeof(string) => (T)(object)v.Values.First().Value,
            var v => v.Values.ToObject<T>(),
        };
    }
}
