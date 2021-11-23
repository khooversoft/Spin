using AisParser;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DataTools.Services
{
    internal class AisStore
    {
        private readonly ILogger<AisStore> _logger;
        private readonly ConcurrentDictionary<string, FileWriter> _output = new ConcurrentDictionary<string, FileWriter>();
        private readonly IReadOnlyList<string> _headers = new List<string>
        {
            "Key",
            "GroupId",
            "SourceType",
            "Timestamp",
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

        //public void Write(NmeaRecord? nmeaRecord)
        //{
        //    if (nmeaRecord == null)
        //    {
        //        _counters.Increment(Counter.WriteSkip);
        //        return;
        //    }

        //    _counters.Increment(Counter.Write);
        //    nmeaRecord.AisMessageType.VerifyNotEmpty("AisMessageType is required");

        //    FileWriter fileWriter = _output.GetOrAdd(
        //        nmeaRecord.AisMessageType,
        //        x => new FileWriter(Path.Combine(StoreFolder, nmeaRecord.AisMessageType + ".tsv"), _headers, _logger)
        //        );

        //    var vector = new StringVector("\t")
        //        .Add(Guid.NewGuid().ToString())
        //        .Add(nmeaRecord.GroupId ?? "*")
        //        .Add(nmeaRecord.SourceType ?? "*")
        //        .Add(nmeaRecord.Timestamp ?? "*")
        //        .Add(nmeaRecord.Quality ?? "*")
        //        .Add(nmeaRecord.Chksum ?? "*")
        //        .Add(nmeaRecord.AisMessage ?? "*")
        //        .Add(nmeaRecord.AisMessageType ?? "*")
        //        .Add(nmeaRecord.AisMessageJson ?? "*");

        //    fileWriter.Write(vector.ToString());
        //}

        public void Write(IEnumerable<NmeaRecord?> nmeaRecords)
        {
            if (nmeaRecords == null) return;

            int totalRecords = nmeaRecords.Count();
            int cleanRecords = nmeaRecords.Count(x => x == null);

            if( cleanRecords > 0)
            {
                _counters.Add(Counter.WriteSkip, cleanRecords);
            }

            _counters.Add(Counter.Write, totalRecords - cleanRecords);

            IEnumerable<IGrouping<string, NmeaRecord>> group = nmeaRecords
                .Where(x => x != null)
                .Select(x => (NmeaRecord)x!)
                .GroupBy(x => x.AisMessageType!);

            foreach (var recordType in group)
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

        private FileWriter GetFileWriter(string recordType) => _output.GetOrAdd(
            recordType,
            x => new FileWriter(Path.Combine(StoreFolder, recordType + ".tsv"), _headers, _logger)
            );
    }
}
