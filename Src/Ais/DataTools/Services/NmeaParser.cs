using DataTools.Application;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DataTools.Services
{
    internal class NmeaParser
    {
        private readonly ILogger<NmeaParser> _logger;
        private readonly AisParser.Parser _parser = new AisParser.Parser();
        private readonly AppOption _appOption;
        private readonly Counters _counters;
        private readonly HashSet<string> _skipMessageTypes;
        private int _multiCount = 0;

        public NmeaParser(AppOption appOption, Counters counters, ILogger<NmeaParser> logger)
        {
            _logger = logger.VerifyNotNull(nameof(logger));
            _appOption = appOption.VerifyNotNull(nameof(appOption));
            _counters = counters.VerifyNotNull(nameof(counters));

            _skipMessageTypes = new HashSet<string>(_appOption.IgnoreTypes, StringComparer.OrdinalIgnoreCase);
        }

        public NmeaRecord? Parse(string line)
        {
            line.VerifyNotEmpty(nameof(line));
            _counters.Increment(Counter.Parse);
            _counters.Set(Counter.ParserFragment, _parser.Fragments.Count);

            Interlocked.Increment(ref _multiCount);
            _counters.SetMulti(_multiCount);

            try
            {
                NmeaRecord nmeaRecord = line[0..2] switch
                {
                    "\\s" => ParseShort(line[1..^0]),
                    "\\g" => ParseLong(line[1..^0]),

                    _ => throw new ArgumentException($"No message header detected - {line}"),
                };

                AisParser.AisMessage? aisMessage = _parser.Parse(nmeaRecord.AisMessage);
                if (aisMessage == null)
                {
                    _counters.Increment(Counter.ParserSkip);
                    return null;
                }

                if(_skipMessageTypes.Contains(aisMessage.MessageType.ToString()))
                {
                    //_counters.Increment(Counter.MsgTypeSkipped);
                    return null;
                }

                string json = aisMessage.ToJson();
                nmeaRecord = nmeaRecord with { AisMessageType = aisMessage.MessageType.ToString(), AisMessageJson = json };

                return nmeaRecord;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Parse failed: {line}, msg={ex.Message}");
                return null;
            }
            finally
            {
                Interlocked.Decrement(ref _multiCount);
            }
        }

        private NmeaRecord ParseShort(string line)
        {
            var sections = line.Split('\\');
            sections.VerifyAssert(x => x.Length == 2, x => $"Invalid format '{line}'");

            (IDictionary<string, string> fields, string chksum) = ParseFields(sections[0]);

            string? sourceType = null;
            string? timestamp = null;
            string? quality = null;

            if (fields.TryGetValue("s", out string? sourceTypeValue))
            {
                sourceType = sourceTypeValue;
            }

            if (fields.TryGetValue("c", out string? timestampValue))
            {
                timestamp = timestampValue;
            }

            if (fields.TryGetValue("q", out string? qualityValue))
            {
                quality = qualityValue;
            }

            return new NmeaRecord
            {
                SourceType = sourceType.VerifyNotEmpty("Source type required"),
                Timestamp = timestamp.VerifyNotEmpty("Timestamp required"),
                Quality = quality.VerifyNotEmpty("Quality required"),
                Chksum = chksum.VerifyNotEmpty("Chksum required"),
                AisMessage = sections[1],
            };
        }

        private NmeaRecord ParseLong(string line)
        {
            var sections = line.Split('\\');
            sections.VerifyAssert(x => x.Length == 2, x => $"Invalid format '{line}'");

            (IDictionary<string, string> fields, string chksum) = ParseFields(sections[0]);

            string? groupId = null;
            string? sourceType = null;
            string? timestamp = null;

            if (fields.TryGetValue("g", out string? groupValue))
            {
                groupId = groupValue;
            }

            if (fields.TryGetValue("s", out string? fieldValue))
            {
                sourceType = fieldValue;
            }

            if (fields.TryGetValue("c", out string? timestampValue))
            {
                timestamp = timestampValue;
            }

            return new NmeaRecord
            {
                GroupId = groupId.VerifyNotEmpty("Group id is required"),
                SourceType = sourceType,
                Timestamp = timestamp,
                Chksum = chksum,
                AisMessage = sections[1],
            };
        }

        private (IDictionary<string, string> fields, string chksum) ParseFields(string line)
        {
            var parts = line.Split('*');

            var fields = parts[0].Split(',')
                .Select(x =>
                {
                    var parts = x.Split(':');
                    parts.VerifyAssert(y => y.Length == 2, x => $"Syntax error for {line}, failed on '{x}");
                    return new KeyValuePair<string, string>(parts[0], parts[1]);
                });

            return (new Dictionary<string, string>(fields), parts[1]);
        }
    }
}

