using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block.Serialization;

public class BlockDocument : IEnumerable<DataItem>
{
    private readonly List<DataItem> _dataItems = new();
    private readonly object _lock = new object();

    public BlockDocument() { }
    public BlockDocument(IEnumerable<DataItem> dataItems) => CommitDataItems = dataItems.ToList();

    public Guid Id { get; set; } = Guid.NewGuid();
    public IReadOnlyList<DataItem> CommitDataItems { get; init; } = Array.Empty<DataItem>();
    public IReadOnlyList<DataItem> DataItems => _dataItems;

    public BlockDocument Add(DataItem dataItem)
    {
        dataItem.Verify();

        lock (_lock)
        {
            // Only add if the value is different
            var currentState = CurrentState();
            if (currentState.Any(x => x.Key.EqualsIgnoreCase(dataItem.Key) && x.Value == dataItem.Value)) return this;

            int index = _dataItems.Count == 0 ? 0 : this.Max(x => x.Index) + 1;
            _dataItems.Add(dataItem with { Index = index });
        }

        return this;
    }

    public BlockDocument Add<T>(T value) where T : class
    {
        IReadOnlyList<DataItem> result = BlockSerializer.Serialize(value);
        result.ForEach(x => Add(x));

        return this;
    }

    public IReadOnlyList<DataItem> CurrentState()
    {
        return this
            .GroupBy(x => x.Key)
            .Select(x => x.OrderByDescending(y => y.Index).First())
            .ToList();
    }

    public IEnumerator<DataItem> GetEnumerator() => CommitDataItems.Concat(DataItems).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


public static class BlockDocumentExtensions
{
    public static BlockDocument Verify(this BlockDocument subject)
    {
        subject.NotNull();
        subject.CommitDataItems.NotNull().ForEach(x => x.Verify());
        subject.DataItems.NotNull().ForEach(x => x.Verify());

        return subject;
    }


}
