using System.Linq;
using Toolbox.BlockDocument.Block;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.BlockDocument
{
    public static class DataBlockExtensions
    {
        //public static DataBlock<T> WithSignature<T>(this DataBlock<T> subject, IPrincipleSignature principleSign)
        //    where T : IBlockType
        //{
        //    return new DataBlock<T>(subject, principleSign);
        //}

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
    }
}