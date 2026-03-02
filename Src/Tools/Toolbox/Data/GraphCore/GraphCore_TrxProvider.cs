using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public partial class GraphCore : ITrxProvider
{
    private TrxSourceRecorder? _recorder;
    private string? _logSequenceNumber;

    public string SourceName => nameof(GraphCore);

    public void AttachRecorder(TrxRecorder trxRecorder) => _recorder = trxRecorder.ForSource(nameof(GraphCore));
    public void DetachRecorder() => _recorder = null;
    public Option<string> GetLogSequenceNumber() => _logSequenceNumber switch { null => StatusCode.NotFound, var lsn => lsn, };
    public void SetLogSequenceNumber(string lsn) => Interlocked.Exchange(ref _logSequenceNumber, lsn.NotEmpty());


    public Task<Option> Checkpoint() => new Option(StatusCode.OK).ToTaskResult();
    public Task<Option> Commit(DataChangeRecord dcr)
    {
        _logSequenceNumber = dcr.GetLastLogSequenceNumber();
        return new Option(StatusCode.OK).ToTaskResult();
    }
    public Task<Option> Restore(string json) => new Option(StatusCode.OK).ToTaskResult();
    public Task<Option> Start() => new Option(StatusCode.OK).ToTaskResult();

    public Task<Option> Recovery(TrxRecoveryScope trxRecoveryScope)
    {
        trxRecoveryScope.NotNull();

        var storeLastLsn = LogSequenceNumber.Parse(_logSequenceNumber);

        foreach (DataChangeRecord record in trxRecoveryScope.Records)
        {
            var lsn = record.GetLastLogSequenceNumber().NotNull().Func(LogSequenceNumber.Parse);
            if (lsn <= storeLastLsn) continue;

            foreach (DataChangeEntry entry in record.Entries.Where(x => x.SourceName == SourceName))
            {
                switch ((entry.Action, entry.TypeName))
                {
                    case (ActionOperator.Add, nameof(Node)):
                        entry.After.NotNull("After value must be present for add operation.");
                        Nodes.TryAdd(entry.After.ToObject<Node>()).ThrowOnError();
                        break;

                    case (ActionOperator.Update, nameof(Node)):
                        entry.After.NotNull("After value must be present for update operation.");
                        Nodes.AddOrUpdate(entry.After.ToObject<Node>()).ThrowOnError(); ;
                        break;

                    case (ActionOperator.Delete, nameof(Node)):
                        entry.Before.NotNull("Before value must be present for delete operation.");
                        Nodes.Remove(entry.ObjectId).ThrowOnError(); ;
                        break;

                    case (ActionOperator.Add, nameof(Edge)):
                        entry.After.NotNull("After value must be present for add operation.");
                        Edges.TryAdd(entry.After.ToObject<Edge>()).ThrowOnError(); ;
                        break;

                    case (ActionOperator.Update, nameof(Edge)):
                        entry.After.NotNull("After value must be present for update operation.");
                        Edges.AddOrUpdate(entry.After.ToObject<Edge>()).ThrowOnError(); ;
                        break;

                    case (ActionOperator.Delete, nameof(Edge)):
                        entry.Before.NotNull("Before value must be present for delete operation.");
                        Edges.Remove(entry.ObjectId).ThrowOnError(); ;
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                _logSequenceNumber = entry.LogSequenceNumber;
            }
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Rollback(DataChangeEntry dcr)
    {
        switch ((dcr.Action, dcr.TypeName))
        {
            case (ActionOperator.Add, nameof(Node)):
                Nodes.Remove(dcr.ObjectId);
                break;

            case (ActionOperator.Update, nameof(Node)):
                dcr.Before.NotNull("Before value must be present for update rollback.");
                Nodes.AddOrUpdate(dcr.Before.ToObject<Node>()).ThrowOnError(); ;
                break;

            case (ActionOperator.Delete, nameof(Node)):
                dcr.Before.NotNull("Before value must be present for delete rollback.");
                Nodes.TryAdd(dcr.Before.ToObject<Node>()).ThrowOnError(); ;
                break;

            case (ActionOperator.Add, nameof(Edge)):
                Edges.Remove(dcr.ObjectId).ThrowOnError(); ;
                break;

            case (ActionOperator.Update, nameof(Edge)):
                dcr.Before.NotNull("Before value must be present for update rollback.");
                Edges.AddOrUpdate(dcr.Before.ToObject<Edge>()).ThrowOnError(); ;
                break;

            case (ActionOperator.Delete, nameof(Edge)):
                dcr.Before.NotNull("Before value must be present for delete rollback.");
                Edges.TryAdd(dcr.Before.ToObject<Edge>()).ThrowOnError(); ;
                break;

            default:
                throw new InvalidOperationException();
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }
}
