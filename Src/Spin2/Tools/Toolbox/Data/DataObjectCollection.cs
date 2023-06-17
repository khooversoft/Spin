using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataObjectCollection : IEnumerable<DataObject>
{
    public DataObjectCollection() { }
    public DataObjectCollection(IEnumerable<DataObject> dataObjects) => Items = new List<DataObject>();

    public IList<DataObject> Items { get; init; } = new List<DataObject>();
    public DataObject this[int index] => Items[index];
    public void Add(params DataObject[] dataObjects) => dataObjects.ForEach(x => Items.Add(x));

    public IEnumerator<DataObject> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Items).GetEnumerator();
}


public static class DataObjectCollectionValidator
{
    public static Validator<DataObjectCollection> Validator { get; } = new Validator<DataObjectCollection>()
        .RuleFor(x => x.Items).NotNull()
        .RuleForEach(x => x.Items).Validate(DataObjectValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this DataObjectCollection subject, ScopeContext context) => Validator.Validate(subject);
}
