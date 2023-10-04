using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataObjectSet
{
    public IReadOnlyDictionary<string, DataObject> Items { get; init; } = new Dictionary<string, DataObject>(StringComparer.OrdinalIgnoreCase);

    public static IValidator<DataObjectSet> Validator { get; } = new Validator<DataObjectSet>()
        .RuleFor(x => x.Items).NotNull()
        .RuleForEach(x => x.Items).Must(x => x.Key.IsNotEmpty() && x.Value.Validate().IsOk(), _ => "Invalid")
        .Build();
}


public static class DataObjectSetExtensions
{
    public static T GetObject<T>(this DataObjectSet subject, string? key = null) where T : new()
    {
        key ??= typeof(T).GetTypeName();
        subject.Items.TryGetValue(key, out var dataObject).Assert(x => x == true, $"key={key} does not exist");

        return dataObject!.ToObject<T>();
    }

    public static DataObjectSet Clone(this DataObjectSet subject) => new DataObjectSet
    {
        Items = subject.Items.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
    };
}
