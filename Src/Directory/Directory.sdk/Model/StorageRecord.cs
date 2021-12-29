using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Directory.sdk.Model
{
    public record StorageRecord
    {
        public string StorageId { get; init; } = null!;

        public string AccountName { get; init; } = null!;

        public string ContainerName { get; init; } = null!;

        public string AccountKey { get; init; } = null!;

        public string? PathRoot { get; init; }
    }

    public static class StorageRecordExtensions
    {
        public static StorageRecord Verify(this StorageRecord subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.StorageId.IsDirectoryIdValid().VerifyAssert(x => x.Valid, x => x.Message);
            subject.AccountName.VerifyNotEmpty($"{nameof(subject.AccountName)} is required");
            subject.ContainerName.VerifyNotEmpty($"{nameof(subject.ContainerName)} is required");
            subject.AccountKey.VerifyNotEmpty($"{nameof(subject.AccountKey)} is required");

            return subject;
        }
    }
}
