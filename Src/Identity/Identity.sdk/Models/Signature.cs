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
}
