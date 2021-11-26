using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DataTools.Services
{
    internal class Tracking
    {
        private readonly AisStore _aisStore;
        private readonly Counters _counters;
        private readonly ILogger<Tracking> _logger;
        private readonly ConcurrentDictionary<string, TrackingItem> _trackedData = new ConcurrentDictionary<string, TrackingItem>(StringComparer.OrdinalIgnoreCase);

        private record TrackingItem
        {
            public string File { get; init; } = null!;
            public string BatchDate { get; init; } = null!;
        }

        private record TrackingData
        {
            public IList<TrackingItem> Data { get; init; } = new List<TrackingItem>();
        }

        public Tracking(AisStore aisStore, Counters counters, ILogger<Tracking> logger)
        {
            _aisStore = aisStore;
            _counters = counters;
            _logger = logger;
        }

        public string? Check(string data)
        {
            if (_trackedData.ContainsKey(data))
            {
                _logger.LogInformation($"Tracking.Check - already processed {data}");
                return null;
            }

            return data;
        }

        public void Add(string data)
        {
            _counters.Increment(Counter.Tracking);

            _trackedData.TryAdd(data, new TrackingItem { File = data, BatchDate = _aisStore.BatchDate })
                .VerifyAssert(x => x == true, "File already exists");
        }

        public void Load()
        {
            _logger.LogInformation($"Loading tracking history file {TrackingFileName}");

            if (!File.Exists(TrackingFileName))
            {
                _logger.LogInformation("Tracking file does not exist, continuing with empty history list");
                return;
            }

            string history = File.ReadAllText(TrackingFileName);
            TrackingData data = history.ToObject<TrackingData>().VerifyNotNull($"Tracking file {TrackingFileName} not valid");

            _logger.LogInformation($"Loading tracking file {TrackingFileName}, count={data.Data.Count}");

            _trackedData.Clear();
            data.Data.ForEach(x => _trackedData[x.File] = x);
        }

        public void Save()
        {
            TrackingData data = new TrackingData
            {
                Data = _trackedData.Values.ToList(),
            };

            string json = data.ToJsonFormat();
            File.WriteAllText(TrackingFileName, json);

            _logger.LogInformation($"Saved tracking history to file {TrackingFileName}");
        }

        private string TrackingFileName => Path.Combine(_aisStore.StoreFolder, "tracking.json");
    }
}
