using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using System;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake.Model
{
    public static class DataLakeExtensions
    {
        public static DataLakePathItem ConvertTo(this PathItem subject)
        {
            subject.VerifyNotNull(nameof(subject));

            return new DataLakePathItem
            {
                Name = subject.Name,
                IsDirectory = subject.IsDirectory,
                LastModified = subject.LastModified,
                ETag = subject.ETag.ToString(),
                ContentLength = subject.ContentLength,
                Owner = subject.Owner,
                Group = subject.Group,
                Permissions = subject.Permissions,
            };
        }

        public static DatalakePathProperties ConvertTo(this PathProperties subject)
        {
            subject.VerifyNotNull(nameof(subject));

            return new DatalakePathProperties
            {
                LastModified = subject.LastModified,
                ContentEncoding = subject.ContentEncoding,
                ETag = subject.ETag.ToString(),
                ContentType = subject.ContentType,
                ContentLength = subject.ContentLength,
                CreatedOn = subject.CreatedOn,
            };
        }

        public static DataLakeServiceClient CreateDataLakeServiceClient(this DataLakeStoreOption azureStoreOption)
        {
            azureStoreOption.VerifyNotNull(nameof(azureStoreOption));

            // Create DataLakeServiceClient using StorageSharedKeyCredentials
            var serviceUri = new Uri($"https://{azureStoreOption.AccountName}.blob.core.windows.net");

            StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(azureStoreOption.AccountName, azureStoreOption.AccountKey);
            return new DataLakeServiceClient(serviceUri, sharedKeyCredential);
        }

        public static void Verify(this DataLakeStoreOption option)
        {
            option.VerifyNotNull(nameof(option));

            option.AccountName.VerifyNotEmpty(nameof(option.AccountName));
            option.AccountKey.VerifyNotEmpty(nameof(option.AccountKey));
            option.ContainerName.VerifyNotEmpty(nameof(option.ContainerName));
        }

        public static void Verify(this DataLakeNamespace subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Namespace.VerifyNotEmpty((nameof(subject.Namespace)));
            subject.Store.Verify();
        }
    }
}