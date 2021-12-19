using DataTools.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DataTools.Activities
{
    /// <summary>
    /// Parse AIS messages and records result in store
    /// </summary>
    internal class ParseActivity
    {
        private readonly FileReader _fileReader;
        private readonly ILogger<ParseActivity> _logger;
        private readonly NmeaParser _parseNmea;
        private readonly AisStore _store;
        private readonly Counters _counters;
        private readonly Tracking _tracking;

        public ParseActivity(FileReader file, AisStore store, NmeaParser parseNmea, Counters counters, Tracking tracking, ILogger<ParseActivity> logger)
        {
            _logger = logger.VerifyNotNull(nameof(logger));

            _fileReader = file.VerifyNotNull(nameof(file));
            _parseNmea = parseNmea.VerifyNotNull(nameof(parseNmea));
            _store = store.VerifyNotNull(nameof(store));
            _counters = counters.VerifyNotNull(nameof(counters));
            _tracking = tracking.VerifyNotNull(nameof(tracking));
        }

        public async Task Parse(string[] files, bool recursive, int? max, CancellationToken token)
        {
            _logger.LogInformation($"Parsing files, Recursive={recursive}, Max={max}, {(files.Select(x => $"File: {x}").ToStringVector(Environment.NewLine))}");

            using var monitorToken = CancellationTokenSource.CreateLinkedTokenSource(token);
            Stopwatch sw = Stopwatch.StartNew();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = false;
                monitorToken.Cancel();
            };

            var parseOption = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 20, BoundedCapacity = 1000 };
            var batchOption = new GroupingDataflowBlockOptions { BoundedCapacity = 1000 };
            var writeOption = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000 };
            var linkOption = new DataflowLinkOptions { PropagateCompletion = true };

            TransformBlock<string, NmeaRecord?> toParse = new TransformBlock<string, NmeaRecord?>(x => _parseNmea.Parse(x), parseOption);
            BatchBlock<NmeaRecord?> toSaveBatch = new BatchBlock<NmeaRecord?>(100, batchOption);
            ActionBlock<NmeaRecord?[]> toSave = new ActionBlock<NmeaRecord?[]>(x => _store.Write(x), writeOption);

            toParse.LinkTo(toSaveBatch, linkOption);
            _ = toParse.Completion.ContinueWith(delegate { toSaveBatch.Complete(); });

            toSaveBatch.LinkTo(toSave, linkOption);
            _ = toSaveBatch.Completion.ContinueWith(delegate { toSave.Complete(); });

            _counters.Clear();
            _tracking.Load();

            _counters.Sampler = x =>
            {
                x.Set(Counter.ToParseIn, toParse.InputCount);
                x.Set(Counter.ToParseOut, toParse.OutputCount);
                x.Set(Counter.ToSaveIn, toParse.InputCount);
                x.Set(Counter.ToSaveOut, toParse.OutputCount);
            };

            // Start the process
            IReadOnlyList<string> readFiles = files
                .SelectMany(x => _fileReader.GetFiles(x!, true))
                .Select(x => _tracking.Check(x))
                .Where(x => x != null)
                .TakeWhile((x, i) => max == null || i < (int)max)
                .ToList()!;

            _counters.Monitor(monitorToken.Token);
            _counters.Set(Counter.FileQueued, readFiles.Count);

            foreach (var file in readFiles)
            {
                if (monitorToken.IsCancellationRequested) break;

                _tracking.Add(file);
                await _fileReader.ReadFile(file, toParse);
            }

            toParse.Complete();
            await toSave.Completion;

            await _counters.Completion(monitorToken.Token);

            _logger.LogInformation("Shutting down");
            monitorToken.Cancel();

            _store.Close();
            _tracking.Save();

            // Report on performance
            sw.Stop();
            _logger.LogInformation($"Final: {_counters.FinalScore(sw.Elapsed)}");
        }
    }
}
