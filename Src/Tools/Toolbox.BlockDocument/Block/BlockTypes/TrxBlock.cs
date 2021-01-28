using System;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.BlockDocument
{
    public class TrxBlock : BlockBase, IBlockType
    {
        public TrxBlock(string referenceId, string transactionType, MaskDecimal4 value, IEnumerable<KeyValuePair<string, string>>? properties = null)
            : base(properties)
        {
            referenceId.VerifyNotEmpty(nameof(referenceId));
            transactionType.VerifyNotEmpty(nameof(transactionType));

            ReferenceId = referenceId.Trim();
            TransactionType = transactionType.Trim();
            Value = value;

            Digest = GetDigest();
        }

        public string ReferenceId { get; }

        public string TransactionType { get; }

        public MaskDecimal4 Value { get; }

        public string Digest { get; }

        public string GetDigest() => $"{ReferenceId}-{TransactionType}-{Value}" + base.ToString()
                .ToBytes()
                .ToSHA256Hash();

        public override bool Equals(object? obj)
        {
            return obj is TrxBlock trxBlock &&
                ReferenceId == trxBlock.ReferenceId &&
                TransactionType == trxBlock.TransactionType &&
                Value == trxBlock.Value &&
                Digest == trxBlock.Digest &&
                base.Equals(trxBlock);
        }

        public override int GetHashCode() => HashCode.Combine(ReferenceId, TransactionType, Value, Digest);

        public static bool operator ==(TrxBlock v1, TrxBlock v2) => v1.Equals(v2);

        public static bool operator !=(TrxBlock v1, TrxBlock v2) => !v1.Equals(v2);
    }
}