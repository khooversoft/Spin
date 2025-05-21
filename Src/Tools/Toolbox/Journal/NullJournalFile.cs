//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Extensions;
//using Toolbox.Journal;
//using Toolbox.Types;

//namespace Toolbox.Journal;

//public class NullJournalFile : IJournalFile, IAsyncDisposable
//{
//    public Task Close() => Task.CompletedTask;

//    public IJournalTrx CreateTransactionContext(string? transactionId = null) => new NullJournalFile().Cast<IJournalTrx>();

//    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

//    public Task<IReadOnlyList<string>> GetFiles(ScopeContext context) => ((IReadOnlyList<string>)Array.Empty<string>()).ToTaskResult();

//    public Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context) => ((IReadOnlyList<JournalEntry>)Array.Empty<JournalEntry>()).ToTaskResult();

//    public Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();
//}


//public class NullJournalTrx : IJournalTrx
//{
//    public string TransactionId => throw new NotImplementedException();

//    public Task Commit() => Task.CompletedTask;
//    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
//    public Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries) => new Option(StatusCode.OK).ToTaskResult();
//}
