using AisParser;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace DataTools.Services
{
    internal class AisStore
    {
        private readonly ILogger<AisStore> _logger;
        private readonly ConcurrentDictionary<string, FileWriter> _output = new ();
        private readonly IReadOnlyList<string> _headers = new List<string>
        {
            "Key",
            "GroupId",
            "SourceType",
            "Timestamp",
            "Date",
            "Quality",
            "Chksum",
            "AisMessage",
            "AisMessageType",
            "AisMessageJson",
        };
        private readonly Counters _counters;

        public AisStore(Counters counters, ILogger<AisStore> logger)
        {
            _logger = logger.VerifyNotNull(nameof(logger));
            _counters = counters;
        }

        public string StoreFolder { get; private set; } = null!;

        public string BatchDate { get; } = DateTime.Now.ToString("yyyyMMdd-HH-mm");

        public AisStore SetStoreFolder(string folder)
        {
            StoreFolder = folder.VerifyNotEmpty(nameof(folder));

            Directory.Exists(StoreFolder)
                .VerifyAssert(x => x == true, x => $"'{x} is not a folder or does not exist");

            return this;
        }

        public Task RestStore()
        {
            Verify();

            _logger.LogInformation($"Reseting store at {StoreFolder}");

            Directory.Delete(StoreFolder, true);
            Directory.CreateDirectory(StoreFolder);
            return Task.CompletedTask;
        }

        public void Close()
        {
            _output.Values
                .ToList()
                .ForEach(x => x.Close());

            _output.Clear();
        }

        public void Write(NmeaRecord?[] nmeaRecords)
        {
            nmeaRecords.VerifyNotNull(nameof(nmeaRecords));

            _counters.Add(Counter.Writing, nmeaRecords.Length);
            _counters.Add(Counter.WriteSkip, nmeaRecords.Count(x => x == null));

            IEnumerable<IGrouping<string, NmeaRecord>> group = nmeaRecords
                .Where(x => x != null)
                .Select(x => (NmeaRecord)x!)
                .GroupBy(x => x.AisMessageType!);

            foreach (IGrouping<string, NmeaRecord> recordType in group)
            {
                FileWriter fileWriter = GetFileWriter(recordType.Key!);

                foreach (NmeaRecord record in recordType)
                {
                    string line = new[]
                    {
                        Guid.NewGuid().ToString(),
                        record.GroupId ?? "*",
                        record.SourceType ?? "*",
                        record.Timestamp ?? "*",
                        ConvertToDateTime(record.Timestamp),
                        record.Quality ?? "*",
                        record.Chksum ?? "*",
                        record.AisMessage ?? "*",
                        record.AisMessageType ?? "*",
                        record.AisMessageJson ?? "*"
                    }.Join("\t");

                    fileWriter.Write(line);
                    _counters.Increment(Counter.Write);
                }
            }
        }

        private void Verify() => StoreFolder.VerifyNotEmpty("SetStoreFolder must be called first");

        private string ConvertToDateTime(string? unixTimestamp)
        {
            if (unixTimestamp == null || !long.TryParse(unixTimestamp, out long timestamp)) return DateTimeOffset.MinValue.ToString("O");

            return ((DateTimeOffset)(UnixDate)timestamp).ToString("yyyy-MM-dd HH:mm:ss");
        }

        private FileWriter GetFileWriter(string recordType) => _output.GetOrAdd(
            recordType,
            x => new FileWriter(fileName: Path.Combine(StoreFolder, recordType + ".tsv"), batchDate: BatchDate, headers: _headers, _logger)
            );
    }
}
