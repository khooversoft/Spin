using ArtifactStore.sdk.Model;
using System;
using System.Security.Cryptography;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Extensions
{
    public static class ArtifactPayloadExtensions
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
    }
}