using ArtifactStore.sdk.Model;
using Identity.sdk.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
    public record Signature
    {
        public IdentityId SignatureId { get; set; } = null!;

        public string Key { get; set; } = null!;


        public static ArtifactId ToArtifactId(IdentityId signatureId)
        {
            signatureId.VerifyNotNull(nameof(signatureId));

            return new ArtifactId($"signature/{signatureId}");
        }
    }

    public static class SignatureExtenions
    {
        public static void Verify(this Signature signature)
        {
            signature.VerifyNotNull(nameof(signature));
            signature.SignatureId.VerifyNotNull(nameof(signature.SignatureId));
            signature.Key.VerifyNotEmpty(nameof(signature.Key));
        }

        public static ArtifactId GetArtifactId(this Signature signature)
        {
            signature.VerifyNotNull(nameof(signature));
            signature.Verify();

            return Signature.ToArtifactId(signature.SignatureId);
        }

        public static Signature ToSignature(this ArtifactPayload artifactPayload)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            return artifactPayload.DeserializeFromArtifactPayload<Signature>();
        }

        public static ArtifactPayload ToArtifactPayload(this Signature signature)
        {
            signature.VerifyNotNull(nameof(signature));

            return signature.ToArtifactPayload(signature.GetArtifactId());
        }
    }
}
