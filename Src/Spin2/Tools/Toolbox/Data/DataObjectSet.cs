using System.Text.Json;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataObjectSet : Dictionary<string, DataObject>
{
    public DataObjectSet() : base(StringComparer.OrdinalIgnoreCase) { }

    public DataObjectSet(IEnumerable<KeyValuePair<string, DataObject>> copyFrom)
        : base(copyFrom, StringComparer.OrdinalIgnoreCase)
    {
    }

    public DataObjectSet(IReadOnlyDictionary<string, object>? fromJsonParse)
        : base(StringComparer.OrdinalIgnoreCase)
    {
        fromJsonParse.NotNull();

        foreach (var item in fromJsonParse)
        {
            this[item.Key] = new DataObject
            {
                Key = item.Key,
                TypeName = item.Key,
                JsonData = item.Value switch
                {
                    JsonElement v => v.ToJson(),
                    var v => v.ToString().NotEmpty(),
                }
            };
        }
    }

    public static IValidator<DataObjectSet> Validator { get; } = new Validator<DataObjectSet>()
        .RuleForEach(x => x.Keys).Must(x => x.IsNotEmpty(), _ => "Invalid")
        .RuleForEach(x => x.Values).Must(x => x.Validate().IsOk(), _ => "Invalid")
        .Build();
}


public static class DataObjectSetExtensions
{
    public static Option Validate(this DataObjectSet subject) => DataObjectSet.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DataObjectSet subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static T GetObject<T>(this DataObjectSet subject, string? key = null)
    {
        key ??= typeof(T).GetTypeName();
        subject.TryGetValue(key, out var dataObject).Assert(x => x == true, $"key={key} does not exist");

        return dataObject!.ToObject<T>();
    }
}
