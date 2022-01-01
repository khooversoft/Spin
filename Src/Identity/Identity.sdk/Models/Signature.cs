//using Identity.sdk.Types;
//using Toolbox.Tools;

//namespace Identity.sdk.Models
//{
//    public record Signature
//    {
//        public IdentityId SignatureId { get; set; } = null!;

//        public string Key { get; set; } = null!;


//        public static ArtifactId ToArtifactId(IdentityId signatureId)
//        {
//            signatureId.VerifyNotNull(nameof(signatureId));

//            return new ArtifactId($"signature/{signatureId}");
//        }
//    }
//}
