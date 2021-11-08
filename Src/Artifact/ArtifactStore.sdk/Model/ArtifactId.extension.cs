using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MessageNet.sdk.Protocol;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Model
{
    public static class ArtifactIdExtensions
    {
        public static ArtifactPayload ToArtifactPayload<T>(this T subject, ArtifactId artifactId) where T : class
        {
            subject.VerifyNotNull(nameof(subject));
            artifactId.VerifyNotNull(nameof(artifactId));

            string json = Json.Default.Serialize(subject);
            return Encoding.UTF8.GetBytes(json).ToArtifactPayload(artifactId);
        }

        public static Content ToContent(this ArtifactId artifactId) => new Content
        {
            ContentType = typeof(ArtifactId).ToString(),
            Data = Json.Default.Serialize(artifactId),
        };
    }
}
