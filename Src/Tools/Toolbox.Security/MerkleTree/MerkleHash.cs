using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Security
{
    public struct MerkleHash
    {
        public MerkleHash(byte[] buffer)
        {
            buffer.VerifyNotNull(nameof(buffer));

            SHA256 sha256 = SHA256.Create();
            Value = sha256.ComputeHash(buffer);
        }

        public MerkleHash(string buffer)
            : this(Encoding.UTF8.GetBytes(buffer))
        {
        }

        public MerkleHash(MerkleHash left, MerkleHash right)
            : this(left.Value.Concat(right.Value).ToArray())
        {
        }

        public IReadOnlyList<byte> Value { get; }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => Convert.ToBase64String(Value.ToArray());

        public override bool Equals(object? obj)
        {
            obj.VerifyNotNull(nameof(obj))
                .VerifyAssert(x => x is MerkleHash, "r-value is not a MerkleHash");

            return Equals((MerkleHash)obj);
        }

        public bool Equals(byte[] hash) => Value.SequenceEqual(hash);

        public bool Equals(MerkleHash hash)
        {
            return Value != null &&
                hash != default &&
                hash.Value != null &&
                Value.SequenceEqual(hash.Value);
        }

        public static bool operator ==(MerkleHash h1, MerkleHash h2) => h1.Equals(h2);

        public static bool operator !=(MerkleHash h1, MerkleHash h2) => !h1.Equals(h2);
    }
}