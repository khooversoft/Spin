using System;
using System.Security.Cryptography;
using System.Text;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Model
{
    public static class Extensions
    {
        public static void Verify(this ArtifactPayload subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Id.VerifyNotEmpty(nameof(subject.Id));
            subject.PackagePayload.VerifyAssert(x => x?.Length > 0, $"{nameof(subject.PackagePayload)} is required");
            subject.Hash.VerifyNotEmpty($"{nameof(subject.Hash)} is required");

            byte[] packagePayload = Convert.FromBase64String(subject.PackagePayload);
            byte[] hash = MD5.Create().ComputeHash(packagePayload);

            Convert.ToBase64String(hash).VerifyAssert(x => x == subject.Hash, "Hash verification failed");
        }

        public static bool IsValid(this ArtifactPayload subject)
        {
            try
            {
                subject.Verify();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static byte[] ToBytes(this ArtifactPayload subject)
        {
            subject.Verify();

            return Convert.FromBase64String(subject.PackagePayload);
        }

        public static ArtifactPayload ToArtifactPayload(this byte[] subject, ArtifactId articleId)
        {
            subject.VerifyAssert(x => x?.Length > 0, $"{nameof(subject)} is empty");
            articleId.VerifyNotNull(nameof(articleId));

            var payload = new ArtifactPayload
            {
                Id = (string)articleId,
                PackagePayload = Convert.ToBase64String(subject),
                Hash = Convert.ToBase64String(MD5.Create().ComputeHash(subject)),
            };

            payload.Verify();
            return payload;
        }

        public static ArtifactPayload ToArtifactPayload<T>(this T subject, ArtifactId artifactId) where T : class
        {
            subject.VerifyNotNull(nameof(subject));
            artifactId.VerifyNotNull(nameof(artifactId));

            string json = Json.Default.Serialize(subject);
            return Encoding.UTF8.GetBytes(json).ToArtifactPayload(artifactId);
        }

        public static T DeserializeFromArtifactPayload<T>(this ArtifactPayload artifactPayload) where T : class
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            byte[] bytes = artifactPayload.ToBytes();
            string json = Encoding.UTF8.GetString(bytes);

            return Json.Default.Deserialize<T>(json)
                .VerifyNotNull($"Failed to deserialize {typeof(T).Name}");
        }
    }
}