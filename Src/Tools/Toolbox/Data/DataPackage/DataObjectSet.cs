using System.Text.Json;
using Toolbox.Extensions;
using Toolbox.Tools;
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

    public DataObjectSet Set(DataObject subject) => this.Action(x => x[subject.Key] = subject);

    public DataObjectSet Set<T>(T value) where T : class
    {
        var data = DataObject.Create(value);
        this[data.Key] = data;
        return this;
    }

    public static DataObjectSet operator +(DataObjectSet subject, DataObject dataObject) => subject.Action(x => subject[dataObject.Key] = dataObject);

    public static IValidator<DataObjectSet> Validator { get; } = new Validator<DataObjectSet>()
        .RuleForEach(x => x.Keys).Must(x => x.IsNotEmpty(), _ => "Invalid")
        .RuleForEach(x => x.Values).Validate(DataObject.Validator)
        .Build();
}


public static class DataObjectSetExtensions
{
    public static Option<IValidatorResult> Validate(this DataObjectSet subject) => DataObjectSet.Validator.Validate(subject);

    public static T GetObject<T>(this DataObjectSet subject, string? key = null, IValidator<T>? validator = null)
    {
        key ??= typeof(T).GetTypeName();
        subject.TryGetValue(key, out var dataObject).Assert(x => x == true, $"key={key} does not exist");

        var value = dataObject!.ToObject<T>();

        if (validator != null)
        {
            var validatorResult = validator.Validate(value);
            if (validatorResult.IsError()) throw new ArgumentException($"Type={typeof(T).FullName} failed validation, error={validatorResult.Error}");
        }

        return value;
    }

    public static Option<T> TryGetObject<T>(this DataObjectSet subject, out T value, string? key = null, IValidator<T>? validator = null)
    {
        value = default!;

        key ??= typeof(T).GetTypeName();
        if (!subject.TryGetValue(key, out var dataObject)) return (StatusCode.NotFound, $"Type {typeof(T).FullName} is not found");

        value = dataObject.ToObject<T>();
        if (validator != null)
        {
            var validatorResult = validator.Validate(value);
            if (validatorResult.IsError()) return validatorResult.ToOptionStatus<T>();
        }

        return value;
    }
}
