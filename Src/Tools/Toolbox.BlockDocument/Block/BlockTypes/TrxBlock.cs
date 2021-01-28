using System;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.BlockDocument
{
    public class TrxBlock : IBlockType
    {
        public TrxBlock(string referenceId, string transactionType, MaskDecimal4 value)
        {
            referenceId.VerifyNotEmpty(nameof(referenceId));
            transactionType.VerifyNotEmpty(nameof(transactionType));

            ReferenceId = referenceId;
            TransactionType = transactionType;
            Value = value;

            Digest = GetDigest();
        }

        public string ReferenceId { get; }

        public string TransactionType { get; }

        public MaskDecimal4 Value { get; }

        // TODO: Need properties

        public string Digest { get; }

        public string GetDigest() => $"{ReferenceId}-{TransactionType}-{Value}"
                .ToBytes()
                .ToSHA256Hash();

        public override bool Equals(object? obj)
        {
            if (obj is TrxBlock trxBlock)
            {
                return ReferenceId == trxBlock.ReferenceId &&
                    TransactionType == trxBlock.TransactionType &&
                    Value == trxBlock.Value &&
                    Digest == trxBlock.Digest;
            }

            return false;
        }

        public override int GetHashCode() => HashCode.Combine(ReferenceId, TransactionType, Value, Digest);

        public static bool operator ==(TrxBlock v1, TrxBlock v2) => v1.Equals(v2);

        public static bool operator !=(TrxBlock v1, TrxBlock v2) => !v1.Equals(v2);
    }
}