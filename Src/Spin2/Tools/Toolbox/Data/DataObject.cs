﻿using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public sealed record DataObject
{
    public string Key { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public IReadOnlyDictionary<string, string> Values { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public bool Equals(DataObject? obj) => obj is DataObject document &&
        Key == document.Key &&
        TypeName == document.TypeName &&
        Values.SequenceEqual(document.Values);

    public override int GetHashCode() => HashCode.Combine(Key, TypeName, Values);

    public static Validator<DataObject> Validator { get; } = new Validator<DataObject>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.TypeName).NotEmpty()
        .RuleFor(x => x.Values).NotNull()
        .Build();
}


public static class DataObjectValidator
{
    public static Option Validate(this DataObject subject) => DataObject.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DataObject subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static DataObject ToDataObject<T>(this T value, string? key = null) where T : class
    {
        IReadOnlyList<KeyValuePair<string, string>> values = value.GetConfigurationValues();

        return new DataObject
        {
            Key = key ?? typeof(T).GetTypeName(),
            TypeName = typeof(T).GetTypeName(),
            Values = values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
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
