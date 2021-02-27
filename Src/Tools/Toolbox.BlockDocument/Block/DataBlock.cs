using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.BlockDocument
{
    public class DataBlock<T> : IDataBlock
        where T : IBlockType
    {
        public DataBlock(string blockType, string blockId, T data, IEnumerable<KeyValuePair<string, string>>? properties = null)
        {
            blockType.VerifyNotEmpty(nameof(blockType));
            blockId.VerifyNotEmpty(nameof(blockId));
            data.VerifyNotNull(nameof(data));

            TimeStamp = UnixDate.UtcNow;
            BlockType = blockType;
            BlockId = blockId;
            Data = data;
            Properties = properties?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();

            Digest = GetDigest();
        }

        public DataBlock(UnixDate timeStamp,
                         string blockType,
                         string blockId,
                         T data,
                         IEnumerable<KeyValuePair<string, string>>? properties = null,
                         string? digest = null,
                         string? jwtSignature = null)
            : this(blockType, blockId, data, properties)
        {
            TimeStamp = timeStamp;
            Digest = digest ?? Digest;
            JwtSignature = jwtSignature;
        }

        // TODO: dateTimeStamp to unix date for this constructor
        public DataBlock(DateTimeOffset dateTimeStamp,
                         string blockType,
                         string blockId,
                         T data,
                         IEnumerable<KeyValuePair<string, string>>? properties = null,
                         string? digest = null,
                         string? jwtSignature = null)
            : this(new UnixDate(dateTimeStamp), blockType, blockId, data, properties, digest, jwtSignature)
        {
        }

        public DataBlock(DataBlock<T> dataBlock)
        {
            dataBlock.VerifyNotNull(nameof(dataBlock));
            dataBlock.Validate();

            TimeStamp = dataBlock.TimeStamp;
            BlockType = dataBlock.BlockType;
            BlockId = dataBlock.BlockId;
            Data = dataBlock.Data;
            Digest = dataBlock.Digest;
            Properties = dataBlock.Properties.ToDictionary(x => x.Key, x => x.Value);
            JwtSignature = dataBlock.JwtSignature;

            Validate();

            GetDigest().VerifyAssert<string, SecurityException>(x => x == dataBlock.Digest, _ => "Copy from block digest does not match new block digest");
        }

        public DataBlock(DataBlock<T> dataBlock, IPrincipleSignature principleSign)
            : this(dataBlock)
        {
            dataBlock.VerifyNotNull(nameof(dataBlock));
            principleSign.VerifyNotNull(nameof(principleSign));

            JwtSignature = principleSign.Sign(Digest);
            Validate();
        }

        public UnixDate TimeStamp { get; }

        public string BlockType { get; }

        public string BlockId { get; }

        public T Data { get; }

        public IReadOnlyDictionary<string, string> Properties { get; }

        public string? JwtSignature { get; }

        public string Digest { get; }

        public void Validate()
        {
            GetDigest().VerifyAssert<string, SecurityException>(x => x == Digest, _ => "Block cannot be verified");
        }

        public string GetDigest()
        {
            var hashes = new string[]
            {
                $"{TimeStamp}-{BlockType}-{BlockId}-".ToBytes().ToSHA256Hash(),
                Properties.Aggregate("", (a, x) => a + $",{x.Key}={x.Value}").ToBytes().ToSHA256Hash(),
                Data.GetDigest(),
            };

            return hashes.ToMerkleHash();
        }

        public override bool Equals(object? obj)
        {
            if (obj is DataBlock<T> dataBlock)
            {
                return TimeStamp == dataBlock.TimeStamp &&
                    BlockType == dataBlock.BlockType &&
                    BlockId == dataBlock.BlockId &&
                    Data.Equals(dataBlock.Data) &&
                    Digest == dataBlock.Digest &&
                    Properties.OrderBy(x => x.Key).SequenceEqual(dataBlock.Properties.OrderBy(x => x.Key));
            }

            return false;
        }

        public override int GetHashCode() => HashCode.Combine(TimeStamp, BlockType, BlockId, Data, Digest, Properties);

        public static bool operator ==(DataBlock<T> v1, DataBlock<T> v2) => v1.Equals(v2);

        public static bool operator !=(DataBlock<T> v1, DataBlock<T> v2) => !v1.Equals(v2);
    }
}