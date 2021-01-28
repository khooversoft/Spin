using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Types
{
    /// <summary>
    /// Unix date is number of seconds from 1970-01-01T00:00:00Z
    /// </summary>
    public struct UnixDate
    {
        public UnixDate(long timeStamp) => TimeStamp = timeStamp;

        public UnixDate(DateTimeOffset utcDate) => TimeStamp = utcDate.ToUnixTimeSeconds();

        public static UnixDate UtcNow => (UnixDate)DateTimeOffset.UtcNow;

        public long TimeStamp { get; }

        public static explicit operator UnixDate(long timeStamp) => new UnixDate(timeStamp);

        public static explicit operator UnixDate(DateTimeOffset dateTimeOffset) => new UnixDate(dateTimeOffset);

        public static implicit operator DateTimeOffset(UnixDate unixDate) => DateTimeOffset.FromUnixTimeSeconds(unixDate.TimeStamp);

        public static implicit operator long(UnixDate unix) => unix.TimeStamp;
    }
}
