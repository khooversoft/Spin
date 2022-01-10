using System.Linq;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block
{
    public static class Extensions
    {
        public static void Add<T>(this BlockChain blockChain, T value, IPrincipleSignature principleSignature)
        {
            blockChain.VerifyNotNull(nameof(blockChain));
            value.VerifyNotNull(nameof(value));
            principleSignature.VerifyNotNull(nameof(principleSignature));

            blockChain += new DataBlockBuilder()
                .SetTimeStamp(UnixDate.UtcNow)
                .SetBlockType(value.GetType().Name)
                .SetBlockId(blockChain.Blocks.Count.ToString())
                .SetData(value.ToJson())
                .SetPrincipleSignature(principleSignature)
                .Build();
        }

        public static BlockChain ConvertTo(this BlockChainModel blockChainModel)
        {
            blockChainModel.VerifyNotNull(nameof(blockChainModel));

            return new BlockChain(blockChainModel.Blocks);
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

        public static void Validate(this DataBlock subject, PrincipleSignature principleSignature)
        {
            subject.VerifyNotNull(nameof(subject));
            principleSignature.VerifyNotNull(nameof(principleSignature));

            subject.Validate();

            JwtTokenDetails? tokenDetails = principleSignature.ValidateSignature(subject.JwtSignature!);
            tokenDetails.VerifyNotNull("Signature validation failed");

            string? digest = tokenDetails
                !.Claims
                .Where(x => x.Type == "blockDigest")
                .Select(x => x.Value)
                .FirstOrDefault();

            (digest == subject.Digest).VerifyAssert<bool, SecurityException>(x => x == true, _ => "Block's digest does not match signature");
        }

        public static void Validate(this BlockChain blockChain, IPrincipleSignatureCollection keyContainer)
        {
            blockChain.VerifyNotNull(nameof(blockChain));
            keyContainer.VerifyNotNull(nameof(keyContainer));

            blockChain.IsValid()
                .VerifyAssert<bool, SecurityException>(x => x == true, _ => "Block chain has linkage is invalid");

            foreach (BlockNode node in blockChain.Blocks)
            {
                string issuer = JwtTokenParser.GetIssuerFromJwtToken(node.BlockData.JwtSignature!)!
                    .VerifyAssert<string, SecurityException>(x => x != null, _ => "Issuer is not found in JWT Signature");

                IPrincipleSignature principleSignature = keyContainer.Get(issuer).VerifyNotNull($"No principle signature found for {issuer}");

                node.BlockData.Validate(principleSignature);
            }
        }
        public static void Validate(this DataBlock dataBlock)
        {
            dataBlock.VerifyNotNull(nameof(dataBlock));
            (dataBlock.Digest == dataBlock.GetDigest()).VerifyAssert<bool, SecurityException>(x => x == true, _ => "Digest does not match");
        }

        public static void Validate(this DataBlock dataBlock, IPrincipleSignature principleSignature)
        {
            principleSignature.VerifyNotNull(nameof(principleSignature));

            dataBlock.Validate();
            principleSignature.ValidateSignature(dataBlock.JwtSignature);
        }
    }
}