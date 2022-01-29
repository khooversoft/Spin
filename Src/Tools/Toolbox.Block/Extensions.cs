using System;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Security.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block
{
    public static class Extensions
    {
        public static async Task Add<T>(this BlockChain blockChain, T value, Func<string, Task<string>> sign)
        {
            blockChain.VerifyNotNull(nameof(blockChain));
            value.VerifyNotNull(nameof(value));
            sign.VerifyNotNull(nameof(sign));

            blockChain += await new DataBlockBuilder()
                .SetTimeStamp(DateTime.UtcNow)
                .SetBlockType(value.GetType().Name)
                .SetBlockId(blockChain.Blocks.Count.ToString())
                .SetData(value.ToJson())
                .SetSign(sign)
                .Build();
        }

        public static BlockChain ConvertTo(this BlockChainModel blockChainModel)
        {
            blockChainModel.VerifyNotNull(nameof(blockChainModel));

            return new BlockChain(blockChainModel.Blocks);
        }

        public static BlockChainModel ConvertTo(this BlockChain blockChain)
        {
            blockChain.VerifyNotNull(nameof(blockChain));

            return new BlockChainModel { Blocks = blockChain.Blocks.ToList() };
        }

        public static string GetDigest(this DataBlock dataBlock)
        {
            dataBlock.VerifyNotNull(nameof(dataBlock));

            var hashes = new string[]
            {
                $"{dataBlock.TimeStamp}-{dataBlock.BlockType}-{dataBlock.BlockId}-".ToBytes().ToSHA256Hash(),
                dataBlock.Properties.Aggregate("", (a, x) => a + $",{x.Key}={x.Value}").ToBytes().ToSHA256Hash(),
                dataBlock.Data.ToBytes().ToSHA256Hash(),
            };

            return hashes.ToMerkleHash();
        }

        public static MerkleTree ToMerkleTree(this BlockChain blockChain)
        {
            return new MerkleTree()
                .Append(blockChain.Blocks.Select(x => x.Digest).ToArray());
        }

        public static void Validate(this DataBlock subject, PrincipalSignature principleSignature)
        {
            subject.VerifyNotNull(nameof(subject));
            principleSignature.VerifyNotNull(nameof(principleSignature));

            subject.Validate();

            JwtTokenDetails? tokenDetails = principleSignature.ValidateSignature(subject.JwtSignature!);
            tokenDetails.VerifyNotNull("Signature validation failed");

            string? digest = tokenDetails!
                .Claims
                .Where(x => x.Type == "blockDigest")
                .Select(x => x.Value)
                .FirstOrDefault();

            (digest == subject.Digest).VerifyAssert<bool, SecurityException>(x => x == true, _ => "Block's digest does not match signature");
        }

        public static void Validate(this BlockChain blockChain, Func<string, IPrincipalSignature> getPrincipleSignature)
        {
            blockChain.VerifyNotNull(nameof(blockChain));
            getPrincipleSignature.VerifyNotNull(nameof(getPrincipleSignature));

            blockChain.IsValid()
                .VerifyAssert<bool, SecurityException>(x => x == true, _ => "Block chain has linkage is invalid");

            foreach (BlockNode node in blockChain.Blocks)
            {
                IPrincipalSignature principleSignature = getPrincipleSignature(node.BlockData.JwtSignature!)
                    .VerifyNotNull($"No principle signature returned");

                node.BlockData.Validate(principleSignature);
            }
        }
        public static void Validate(this DataBlock dataBlock)
        {
            dataBlock.VerifyNotNull(nameof(dataBlock));
            (dataBlock.Digest == dataBlock.GetDigest()).VerifyAssert<bool, SecurityException>(x => x == true, _ => "Digest does not match");
        }

        public static void Validate(this DataBlock dataBlock, IPrincipalSignature principleSignature)
        {
            principleSignature.VerifyNotNull(nameof(principleSignature));

            dataBlock.Validate();
            principleSignature.ValidateSignature(dataBlock.JwtSignature);
        }
    }
}