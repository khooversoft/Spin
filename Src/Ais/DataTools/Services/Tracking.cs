using Microsoft.Extensions.Logging;
using System;
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
        private TrackingData _data = new TrackingData();
        private readonly object _lock = new object();

        private class TrackingData
        {
            public HashSet<string> Data { get; set; } = new HashSet<string>();
        }

        public Tracking(AisStore aisStore, Counters counters, ILogger<Tracking> logger)
        {
            _aisStore = aisStore;
            _counters = counters;
            _logger = logger;
        }

        public string? Check(string data)
        {
            lock (_lock)
            {
                if (Lookup(data)) return null;

                _counters.Increment(Counter.Tracking);

                _data.Data.Add(data)
                    .VerifyAssert(x => x == true, "File already exists");

                return data;
            }
        }

        public void Load()
        {
            _logger.LogInformation($"Loading tracking history file {TrackingFileName}");

            if( !File.Exists(TrackingFileName) )
            {
                _logger.LogInformation("Tracking file does not exist, continuing with empty history list");
                return;
            }

            string history = File.ReadAllText(TrackingFileName);
            _data = history.ToObject<TrackingData>().VerifyNotNull($"Tracking file {TrackingFileName} not valid");

            _logger.LogInformation($"Loading tracking file {TrackingFileName}, count={_data.Data.Count}");
        }

        public void Save()
        {
            string json = _data.ToJsonFormat();
            File.WriteAllText(TrackingFileName, json);

            _logger.LogInformation($"Saved tracking history to file {TrackingFileName}");
        }

        private bool Lookup(string data) => _data.Data.Contains(data);

        private string TrackingFileName => Path.Combine(_aisStore.StoreFolder, "tracking.json");
    }
}
