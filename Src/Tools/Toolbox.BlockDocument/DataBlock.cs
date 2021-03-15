using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.BlockDocument
{
    public class DataBlock : IEquatable<DataBlock?>
    {
        public DataBlock() { }

        public long TimeStamp { get; init; } = UnixDate.UtcNow;

        public string BlockType { get; init; } = null!;

        public string BlockId { get; init; } = null!;

        public string Data { get; init; } = null!;

        public IReadOnlyDictionary<string, string> Properties { get; init; } = null!;

        public string JwtSignature { get; init; } = null!;

        public string Digest { get; init; } = null!;

        public override bool Equals(object? obj) => Equals(obj as DataBlock);

        public bool Equals(DataBlock? other)
        {
            return other != null &&
                TimeStamp == other.TimeStamp &&
                BlockType == other.BlockType &&
                BlockId == other.BlockId &&
                Data == other.Data &&
                Properties.IsEqual(other.Properties) &&
                JwtSignature == other.JwtSignature &&
                Digest == other.Digest;
        }

        public override int GetHashCode() => HashCode.Combine(TimeStamp, BlockType, BlockId, Data, Properties, JwtSignature, Digest);

        public static bool operator ==(DataBlock? left, DataBlock? right) => EqualityComparer<DataBlock>.Default.Equals(left, right);

        public static bool operator !=(DataBlock? left, DataBlock? right) => !(left == right);
    }
}
