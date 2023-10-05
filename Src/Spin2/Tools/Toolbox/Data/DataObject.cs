using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public sealed record DataObject
{
    public string Key { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public string JsonData { get; init; } = null!;
    public string? Tags { get; init; }

    public bool Equals(DataObject? obj) => obj is DataObject document &&
        Key == document.Key &&
        TypeName == document.TypeName &&
        JsonData == document.JsonData &&
        Tags == document.Tags;

    public override int GetHashCode() => HashCode.Combine(Key, TypeName, JsonData);

    public static Validator<DataObject> Validator { get; } = new Validator<DataObject>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.TypeName).NotEmpty()
        .RuleFor(x => x.JsonData).NotNull()
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
        return new DataObject
        {
            Key = key ?? typeof(T).GetTypeName(),
            TypeName = typeof(T).GetTypeName(),
            JsonData = value.ToJson(),
        };
    }

    public static T ToObject<T>(this DataObject dataObject)
    {
        return dataObject switch
        {
            var v when typeof(T) == typeof(string) => (T)(object)v.JsonData,
            var v => v.JsonData.ToObject<T>().NotNull(),
        };
    }
}
