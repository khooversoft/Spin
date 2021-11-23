using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace DataTools.Services
{
    internal record NmeaRecord
    {
        public string? GroupId { get; set; }
        public string? SourceType { get; init; } = null!;
        public string? Timestamp { get; init; }
        public string? Quality { get; init; }
        public string Chksum { get; init; } = null!;
        public string AisMessage { get; init; } = null!;

        public string? AisMessageType { get; init; }
        public string? AisMessageJson { get; init; }
    }
}
