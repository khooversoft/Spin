//using KGraphCmd.Application;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace KGraphCmd.Commands;

//internal static class JournalTool
//{
//    public static async Task Display(IJournalFile journalFile, bool monitor, int lastNumber, bool fullDump, string? lsn, ScopeContext context)
//    {
//        var hashLsn = new HashSet<string>();

//        while (!context.CancellationToken.IsCancellationRequested)
//        {
//            await ReadJournal(journalFile, hashLsn, lastNumber, fullDump, lsn, context);
//            if (!monitor) return;

//            char triggerChr = await InputTool.WaitForInput(async () => await ReadJournal(journalFile, hashLsn, lastNumber, fullDump, lsn, context), context.CancellationToken);
//            if (triggerChr == 'q') return;

//            var match = InputTool.GetUserCommand(context.CancellationToken, "continue", "quit", "reset");
//            switch (match)
//            {
//                case "continue": continue;
//                case "quit": return;

//                case "reset":
//                    hashLsn.Clear();
//                    continue;

//                default: return;
//            }
//        }
//    }

//    private static async Task ReadJournal(IJournalFile journalFile, HashSet<string> hashLsn, int lastNumber, bool fullDump, string? lsn, ScopeContext context)
//    {
//        IReadOnlyList<JournalEntry> entries = await journalFile.ReadJournals(context);
//        context.LogTrace("Read journals, count={count}", entries.Count);

//        IReadOnlyList<string> newEntries = entries
//            .Where(x => lsn == null || x.LogSequenceNumber.Like(lsn))
//            .TakeLast(lastNumber)
//            .Select(x => x.LogSequenceNumber)
//            .Except(hashLsn)
//            .ToArray();

//        newEntries.ForEach(X => hashLsn.Add(X));

//        DataFormatType dataFormatType = fullDump ? DataFormatType.Full : DataFormatType.Single;

//        var details = newEntries
//            .Join(entries, x => x, x => x.LogSequenceNumber, (lsn, entry) => entry)
//            .Select(x => DataFormatTool.Formats.Format(x, dataFormatType).ToLoggingFormat())
//            .ToArray();

//        details.ForEach(x => context.LogInformation(x));
//    }
//}
