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
    public enum Counter
    {
        FileQueued = 0,
        FileRead,
        FileLine,
        Tracking,
        Parse,
        Write,
        WriteSkip,
        ParserSkip,
        //MsgTypeSkipped,
        ParserFragment,
        ToParseIn,
        ToParseOut,
        ToSaveIn,
        ToSaveOut,
    }

    internal class Counters
    {
        private readonly ILogger<Counters> _logger;
        private readonly int[] _counters = new int[Enum.GetValues(typeof(Counter)).Length];
        private readonly int[] _LastCounters = new int[Enum.GetValues(typeof(Counter)).Length];
        private readonly MovingAverage _parseMultiAvg = new MovingAverage(128);
        private int _monitorRunning = 0;

        public Counters(ILogger<Counters> logger)
        {
            _logger = logger.VerifyNotNull(nameof(logger));
        }

        public Action<Counters>? Sampler { get; set; }

        public void Increment(Counter name) => Interlocked.Increment(ref _counters[(int)name]);

        public void Add(Counter name, int count) => Interlocked.Add(ref _counters[(int)name], count);

        public void Set(Counter name, int count) => Interlocked.Exchange(ref _counters[(int)name], count);

        public void SetMulti(int count) => _parseMultiAvg.Add(count);

        public int Get(Counter name) => _counters[(int)name];

        public int GetLast(Counter name) => _LastCounters[(int)name];

        public void Clear() => Array.Clear(_counters);

        public override string ToString() => Enum.GetNames(typeof(Counter))
            .Zip(_counters, (name, value) => $"{name}={value:n0}")
            .Append($"TotalOutput={GetTotalOutput():n0}")
            .Append($"SignalDelta={GetSignalDelta():n0}")
            .Join(", ");

        public string ToString(TimeSpan span) => Enum.GetNames(typeof(Counter))
            .Zip(_counters, (name, value) => $"{name}={value:n0}")
            .Append($"TotalOutput={GetTotalOutput():n0}")
            .Append($"SignalDelta={GetSignalDelta():n0}")
            .Append($"ReadTps={GetReadTps(span):n2}")
            .Append($"ParseTps={GetParseTps(span):n2}")
            .Append($"WriteTps={GetWriteTps(span):n2}")
            .Append($"ParseMulti={_parseMultiAvg.ComputeAverage():n2}")
            .Join(", ");

        internal void Monitor(CancellationToken token)
        {
            var currentState = Interlocked.CompareExchange(ref _monitorRunning, 1, 0);
            if (currentState == 1) return;

            DateTime startTime = DateTime.Now;
            DateTime clock = DateTime.Now;

            _ = Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Monitoring started");

                    while (!token.IsCancellationRequested)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));

                        DateTime now = DateTime.Now;
                        TimeSpan elapsed = now - clock;
                        clock = now;

                        Sampler?.Invoke(this);
                        _logger.LogInformation($"[{(DateTime.Now - startTime).ToString("c")}] Monitor: {ToString(elapsed)}");

                        Array.Copy(_counters, _LastCounters, _counters.Length);
                    }
                }
                finally
                {
                    Sampler?.Invoke(this);
                    _logger.LogInformation($"Monitor exiting: {ToString()}");
                    _monitorRunning = 0;
                }
            });
        }

        public Task Completion(CancellationToken token)
        {
            var tcs = new TaskCompletionSource();

            _ = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    int fileQueue = Get(Counter.FileQueued);
                    int fileRead = Get(Counter.FileRead);
                    int lineCount = Get(Counter.FileLine);
                    int outputCount = Get(Counter.Write);
                    int outputSkipCount = Get(Counter.WriteSkip);

                    if (outputCount == 0) continue;

                    bool fileTest = fileQueue == fileRead;
                    bool lineTest = lineCount == (outputCount + outputSkipCount);

                    if (fileTest && lineTest)
                    {
                        string line = new[]
                        {
                            "Exiting Counters:Completion",
                            $"lineCount={lineCount:n0}",
                            $"FileQueue={fileQueue:n0}",
                            $"FileRead={fileRead:n0}",
                            $"(outputCount={outputCount:n0} + outputSkipCount={outputSkipCount:n0}) = {outputCount + outputSkipCount:n0}",
                            ToString(),
                        }.Join(Environment.NewLine);

                        _logger.LogInformation(line);
                        break;
                    }
                }

                tcs.SetResult();
            });

            return tcs.Task;
        }

        private int GetTotalOutput() => Get(Counter.Write) + Get(Counter.WriteSkip);

        private int GetSignalDelta() => Get(Counter.FileLine) - (Get(Counter.Write) + Get(Counter.WriteSkip));

        private double GetReadTps(TimeSpan span) => (Get(Counter.FileLine) - GetLast(Counter.FileLine)) / span.TotalSeconds;
        
        private double GetParseTps(TimeSpan span) => (Get(Counter.Parse) - GetLast(Counter.Parse)) / span.TotalSeconds;
        
        private double GetWriteTps(TimeSpan span) => (Get(Counter.Write) - GetLast(Counter.Write)) / span.TotalSeconds;
    }
}
