using System;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.BlockDocument
{
    public class HeaderBlock : IBlockType
    {
        public HeaderBlock(string description)
            : this(UnixDate.UtcNow, description)
        {
        }

        public HeaderBlock(UnixDate timeStamp, string description)
        {
            description.VerifyNotEmpty(nameof(description));

            TimeStamp = timeStamp;
            Description = description;

            Digest = GetDigest();
        }

        public UnixDate TimeStamp { get; }

        public string Description { get; }

        public string Digest { get; }

        public string GetDigest() => $"{TimeStamp}-{Description}"
                .ToBytes()
                .ToSHA256Hash();

        public override bool Equals(object? obj)
        {
            if (obj is HeaderBlock headerBlock)
            {
                return TimeStamp == headerBlock.TimeStamp &&
                    Description == headerBlock.Description &&
                    Digest == headerBlock.Digest;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TimeStamp, Description, Digest);
        }

        public static bool operator ==(HeaderBlock v1, HeaderBlock v2) => v1.Equals(v2);

        public static bool operator !=(HeaderBlock v1, HeaderBlock v2) => !v1.Equals(v2);
    }
}