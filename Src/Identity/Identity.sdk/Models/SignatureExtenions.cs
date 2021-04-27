using ArtifactStore.sdk.Model;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
    public static class SignatureExtenions
    {
        public static void Verify(this Signature signature)
        {
            signature.VerifyNotNull(nameof(signature));
            signature.SignatureId.VerifyNotNull(nameof(signature.SignatureId));
            signature.Key.VerifyNotEmpty(nameof(signature.Key));
        }

        public static bool IsValid(this Signature signature) =>
            signature != null &&
            signature.SignatureId != null;

        public static ArtifactId ToArtifactId(this Signature signature)
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

            return signature.ToArtifactPayload(signature.ToArtifactId());
        }
    }
}
