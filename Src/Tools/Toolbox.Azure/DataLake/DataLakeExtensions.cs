using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using System;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake;

public static class DatalakeExtensions
{
    public static DatalakePathItem ConvertTo(this PathItem subject)
    {
        subject.VerifyNotNull(nameof(subject));

        return new DatalakePathItem
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

    public static DatalakePathProperties ConvertTo(this PathProperties subject, string path)
    {
        subject.VerifyNotNull(nameof(subject));
        path.VerifyNotEmpty(nameof(path));

        return new DatalakePathProperties
        {
            Path = path,
            LastModified = subject.LastModified,
            ContentEncoding = subject.ContentEncoding,
            ETag = subject.ETag,
            ContentType = subject.ContentType,
            ContentLength = subject.ContentLength,
            CreatedOn = subject.CreatedOn,
        };
    }

    public static DataLakeServiceClient CreateDataLakeServiceClient(this DatalakeStoreOption azureStoreOption)
    {
        azureStoreOption.VerifyNotNull(nameof(azureStoreOption));

        // Create DataLakeServiceClient using StorageSharedKeyCredentials
        var serviceUri = new Uri($"https://{azureStoreOption.AccountName}.blob.core.windows.net");

        StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(azureStoreOption.AccountName, azureStoreOption.AccountKey);
        return new DataLakeServiceClient(serviceUri, sharedKeyCredential);
    }
}
