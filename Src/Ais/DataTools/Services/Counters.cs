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
        Parser,
        ParserOut,
        ParserSkip,
        ParserError,
        Writing,
        Write,
        WriteSkip,
        MsgTypeSkipped,
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
            .Zip(_counters, (name, value) => $"{name,15}={value,15:n0}")
            .Append($"{"TotalOutput",15}={GetTotalOutput(),15:n0}")
            .Append($"{"SignalDelta",15}={GetSignalDelta(),15:n0}")
            .Chunk(7)
            .Select(x => x.Join(", "))
            .Join(Environment.NewLine);

        public string ToString(TimeSpan span) =>
            new string[]
            {
                new string[]
                {
                    Fmt(Counter.FileQueued),
                    Fmt(Counter.FileRead),
                    Fmt(Counter.Tracking),
                    Fmt(Counter.FileLine),
                    Fmt("ReadTps", GetReadTps(span)),
                    Fmt("TotalOutput", GetTotalOutput()),
                    Fmt("SignalDelta", GetSignalDelta()),
                }.Join(", "),

                new string[]
                {
                    Fmt(Counter.Parser),
                    Fmt(Counter.ParserOut),
                    Fmt(Counter.ParserError),
                    Fmt(Counter.ParserSkip),
                    Fmt(Counter.MsgTypeSkipped),
                    Fmt(Counter.ParserFragment),
                    Fmt("ParseTps", GetParseTps(span)),
                    Fmt("ParseMulti", _parseMultiAvg.ComputeAverage()),
                }.Join(", "),

                new string[]
                {
                    Fmt(Counter.Writing),
                    Fmt(Counter.Write),
                    Fmt(Counter.WriteSkip),
                    Fmt("WriteTps", GetWriteTps(span)),
                }.Join(", "),

                new string[]
                {
                    Fmt(Counter.ToParseIn),
                    Fmt(Counter.ToParseOut),
                    Fmt(Counter.ToSaveIn),
                    Fmt(Counter.ToSaveOut),
                }.Join(", "),

            }.Join(Environment.NewLine);

        private string Fmt(Counter label) => $"{label,14}={Get(label),18:n0}";
        private string Fmt(string label, double value) => $"{label,14}={value,18:n2}";

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
                        Thread.Sleep(TimeSpan.FromSeconds(10));

                        DateTime now = DateTime.Now;
                        TimeSpan elapsed = now - clock;
                        clock = now;

                        Sampler?.Invoke(this);
                        _logger.LogInformation($"[{(DateTime.Now - startTime).ToString("c")}] Monitor:{Environment.NewLine}{ToString(elapsed)}");

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
            DateTime? _lastZero = null;

            _ = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    if (Get(Counter.Write) == 0) continue;

                    bool fileTest = Get(Counter.FileQueued) == Get(Counter.FileRead);
                    bool lineTest = Get(Counter.FileLine) == Get(Counter.Write);

                    bool zeroTest = (Get(Counter.ToParseIn) + Get(Counter.ToParseOut) + Get(Counter.ToSaveIn) + Get(Counter.ToSaveOut)) == 0;
                    if (!zeroTest)
                    {
                        _lastZero = null;
                        continue;
                    }

                    zeroTest = DateTime.Now - (_lastZero ??= DateTime.Now) > TimeSpan.FromSeconds(10);
                    if (zeroTest) _logger.LogInformation("Existing for zero test");

                    if ((fileTest && lineTest) || (fileTest && zeroTest))
                    {
                        string line = new[]
                        {
                            "Exiting Counters:Completion",
                            $"fileTest={fileTest}",
                            $"lineTest={lineTest}",
                            $"zeroTest={zeroTest}",
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

        private double GetParseTps(TimeSpan span) => (Get(Counter.Parser) - GetLast(Counter.Parser)) / span.TotalSeconds;

        private double GetWriteTps(TimeSpan span) => (Get(Counter.Write) - GetLast(Counter.Write)) / span.TotalSeconds;
    }
}
