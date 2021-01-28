using System;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.BlockDocument
{
    public class HeaderBlock : BlockBase, IBlockType
    {
        public HeaderBlock(string description, IEnumerable<KeyValuePair<string, string>>? properties = null)
            : this(UnixDate.UtcNow, description)
        {
        }

        public HeaderBlock(UnixDate timeStamp, string description, IEnumerable<KeyValuePair<string, string>>? properties = null)
            : base(properties)
        {
            description.VerifyNotEmpty(nameof(description));

            TimeStamp = timeStamp;
            Description = description.Trim();

            Digest = GetDigest();
        }

        public UnixDate TimeStamp { get; }

        public string Description { get; }

        public string Digest { get; }

        public string GetDigest() => $"{TimeStamp}-{Description}" + base.ToString()
                .ToBytes()
                .ToSHA256Hash();

        public override bool Equals(object? obj)
        {
            return obj is HeaderBlock headerBlock &&
                TimeStamp == headerBlock.TimeStamp &&
                Description == headerBlock.Description &&
                Digest == headerBlock.Digest &&
                base.Equals(headerBlock);
        }

        public override int GetHashCode() => HashCode.Combine(TimeStamp, Description, Digest);

        public static bool operator ==(HeaderBlock v1, HeaderBlock v2) => v1.Equals(v2);

        public static bool operator !=(HeaderBlock v1, HeaderBlock v2) => !v1.Equals(v2);
    }
}