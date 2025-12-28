// Copyright (c) Kelvin Hoover.  All rights Reserved.
// Licensed under MIT license

using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class ConcurrentMap
{
    public const string SourceName = "concurrentMap";
}


public partial class ConcurrentMap<TKey, TValue> : ITrxProvider
    where TKey : notnull
    where TValue : notnull
{
    private TrxSourceRecorder? _recorder;

    public void AttachRecorder(TrxRecorder trxRecorder)
    {
        _recorder.Assert(x => x == null, "Transaction recorder is already attached.");
        _recorder = trxRecorder.NotNull().ForSource(ConcurrentMap.SourceName);
    }

    public string SourceName => ConcurrentMap.SourceName;

    public void DetachRecorder() => _recorder = null;

    public Task<Option> Start() => new Option(StatusCode.OK).ToTaskResult();
    public Task<Option> Commit() => new Option(StatusCode.OK).ToTaskResult();

    public Task<Option> Rollback(DataChangeEntry entry)
    {
        switch (entry.Action)
        {
            case ChangeOperation.Add:
                {
                    entry.After.HasValue.BeTrue("After value must be present for add operation.");

                    var addedValue = entry.After!.Value.ToObject<TValue>();
                    TKey key = _keySelector(addedValue);
                    _primaryIndex.TryRemove(key, out _);
                    foreach (var index in _secondaryIndexCollection.Providers) index.Remove(addedValue);
                }
                break;

            case ChangeOperation.Delete:
                {
                    // Undo delete: restore the item
                    entry.Before.HasValue.BeTrue("After value must be present for add operation.");

                    var deletedValue = entry.Before!.Value.ToObject<TValue>();
                    TKey key = _keySelector(deletedValue);
                    _primaryIndex[key] = deletedValue;
                    foreach (var index in _secondaryIndexCollection.Providers) index.Set(deletedValue);
                }
                break;

            case ChangeOperation.Update:
                {
                    entry.After.HasValue.BeTrue("After value must be present for add operation.");
                    entry.Before.HasValue.BeTrue("After value must be present for add operation.");

                    var addedValue = entry.After!.Value.ToObject<TValue>();
                    var previousValue = entry.Before!.Value.ToObject<TValue>();

                    // Undo update: restore previous value
                    TKey key = _keySelector(previousValue);
                    _primaryIndex[key] = previousValue;
                    foreach (var index in _secondaryIndexCollection.Providers) index.Set(previousValue);
                }
                break;
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }
}
