using System;
using System.Security.Cryptography;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Model
{
    public static class ArtifactPayloadExtensions
    {
        public static void Verify(this ArtifactPayload subject) => subject.IsValid().VerifyAssert(x => x.valid == true, x => x.msg);

        public static (bool valid, string msg) IsValid(this ArtifactPayload subject)
        {
            if (subject == null) return (false, $"{nameof(subject)} is null");
            if (subject.Id == null) return (false, $"{nameof(subject.Id)} is required");
            if (!(subject.PackagePayload?.Length > 0)) return (false, $"{nameof(subject.PackagePayload)} is required");


            byte[] packagePayload = Convert.FromBase64String(subject.PackagePayload!);
            byte[] hash = MD5.Create().ComputeHash(packagePayload);
            if(Convert.ToBase64String(hash) != subject.Hash) return (false, "Hash verification failed");

            return (true, "Verified");
        }

        public static byte[] PayloadToBytes(this ArtifactPayload subject)
        {
            subject.Verify();

            return Convert.FromBase64String(subject.PackagePayload);
        }

        public static T DeserializePayload<T>(this ArtifactPayload artifactPayload) where T : class
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            byte[] bytes = artifactPayload.PayloadToBytes();
            string data = bytes.BytesToString();

            if (typeof(T) == typeof(string)) return (T)(object)data;

            return Json.Default.Deserialize<T>(data)
                .VerifyNotNull($"Failed to deserialize {typeof(T).Name}");
        }
    }
}
