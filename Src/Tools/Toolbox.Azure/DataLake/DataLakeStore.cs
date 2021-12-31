using Azure;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public class DatalakeStore : IDatalakeStore
    {
        private readonly DataLakeFileSystemClient _fileSystem;
        private readonly DatalakeStoreOption _azureStoreOption;
        private readonly ILogger<DatalakeStore> _logger;
        private readonly DataLakeServiceClient _serviceClient;

        public DatalakeStore(DatalakeStoreOption azureStoreOption, ILogger<DatalakeStore> logger)
        {
            _azureStoreOption = azureStoreOption.Verify();
            _logger = logger.VerifyNotNull(nameof(logger));

            _serviceClient = azureStoreOption.CreateDataLakeServiceClient();

            // Get a reference to a file system (container)
            _fileSystem = _serviceClient.GetFileSystemClient(azureStoreOption.ContainerName);
        }

        public async Task Append(string path, byte[] data, CancellationToken token = default)
        {
            path = WithBasePath(path);
            _logger.LogTrace($"Appending to {path}, data.Length={data.Length}");

            data
                .VerifyNotNull(nameof(data))
                .VerifyAssert(x => x.Length > 0, $"{nameof(data)} length must be greater then 0");

            using var memoryBuffer = new MemoryStream(data.ToArray());

            try
            {
                DatalakePathProperties properties = await GetPathProperties(path, token);

                DataLakeFileClient file = _fileSystem.GetFileClient(path);

                await file.AppendAsync(memoryBuffer, properties.ContentLength, cancellationToken: token);
                await file.FlushAsync(properties.ContentLength + data.Length);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
            {
                await Write(path, data, true, token: token);
            }
            catch (TaskCanceledException) { }
        }

        public async Task<bool> Delete(string path, ETag? eTag = null, CancellationToken token = default)
        {
            path = WithBasePath(path);
            _logger.LogTrace($"Deleting to {path}, ETag={eTag}");

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.DeleteIfExistsAsync(cancellationToken: token);

            return response.Value;
        }

        public async Task DeleteDirectory(string path, CancellationToken token = default)
        {
            path = WithBasePath(path);
            _logger.LogTrace($"Deleting directory {path}");

            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            await directoryClient.DeleteAsync(cancellationToken: token);
        }

        public async Task<bool> Exist(string path, CancellationToken token = default)
        {
            path = WithBasePath(path);
            _logger.LogTrace($"Is path {path} exist");

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.ExistsAsync(token);
            return response.Value;
        }

        public async Task<DatalakePathProperties> GetPathProperties(string path, CancellationToken token = default)
        {
            path = WithBasePath(path);
            _logger.LogTrace($"Getting path {path} properties");

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            return (await file.GetPropertiesAsync(cancellationToken: token))
                .Value
                .ConvertTo(path);
        }

        public async Task Read(string path, Stream toStream, CancellationToken token = default)
        {
            path = WithBasePath(path);
            toStream.VerifyNotNull(nameof(toStream));
            _logger.LogTrace($"Reading {path} to stream");

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            await file.ReadToAsync(toStream, cancellationToken: token);
        }

        public async Task<byte[]?> Read(string path, CancellationToken token = default) => (await ReadWithTag(path, token)).Data;

        public async Task<(byte[]? Data, ETag? Etag)> ReadWithTag(string path, CancellationToken token = default)
        {
            path = WithBasePath(path);
            _logger.LogTrace($"Reading {path}");

            try
            {
                DataLakeFileClient file = _fileSystem.GetFileClient(path);
                Response<FileDownloadInfo> response = await file.ReadAsync(token);

                using MemoryStream memory = new MemoryStream();
                await response.Value.Content.CopyToAsync(memory);

                return (memory.ToArray(), response.Value.Properties.ETag);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
            {
                return (null, null);
            }
        }

        public async Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default)
        {
            queryParameter ??= new QueryParameter();
            queryParameter = queryParameter with { Filter = WithBasePath(queryParameter.Filter) };
            _logger.LogTrace($"Searching {queryParameter}");

            var list = new List<DatalakePathItem>();

            int index = -1;
            try
            {
                await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(queryParameter.Filter, queryParameter.Recursive, cancellationToken: token))
                {
                    index++;
                    if (index < queryParameter.Index) continue;

                    DatalakePathItem datalakePathItem = pathItem.ConvertTo();

                    list.Add(datalakePathItem);
                    if (list.Count >= queryParameter.Count) break;
                }

                return list
                    .Select(x => x with { Name = RemoveBaseRoot(x.Name) })
                    .ToList();
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
            {
                return list;
            }
        }

        public async Task<ETag> Write(Stream fromStream, string toPath, bool overwrite, ETag? eTag = null, CancellationToken token = default)
        {
            toPath = WithBasePath(toPath);
            _logger.LogTrace($"Writing from stream to {toPath}");

            fromStream.VerifyNotNull(nameof(fromStream));

            return await Upload(toPath, fromStream, overwrite, eTag, token);
        }

        public async Task<ETag> Write(string path, byte[] data, bool overwrite, ETag? eTag = null, CancellationToken token = default)
        {
            path = WithBasePath(path);
            _logger.LogTrace($"Writing to {path}, data.Length={data.Length}, eTag={eTag}");

            data
                .VerifyNotNull(nameof(data))
                .VerifyAssert(x => x.Length > 0, $"{nameof(data)} length must be greater then 0");

            _logger.LogTrace($"{nameof(Write)} to {path}");
            using var memoryBuffer = new MemoryStream(data.ToArray());

            return await Upload(path, memoryBuffer, overwrite, eTag, token);
        }

        private string WithBasePath(string? path) => _azureStoreOption.BasePath + (_azureStoreOption.BasePath.IsEmpty() ? string.Empty : "/") + path.VerifyNotEmpty($"{nameof(path)} is required");

        private string RemoveBaseRoot(string path)
        {
            string newPath = path.Substring(_azureStoreOption.BasePath?.Length ?? 0);
            if (newPath.StartsWith("/")) newPath = newPath.Substring(1);

            return newPath;
        }

        private async Task<ETag> Upload(string path, Stream fromStream, bool overwrite, ETag? eTag, CancellationToken token)
        {
            Response<PathInfo> result;

            DataLakeFileClient file = _fileSystem.GetFileClient(path);

            if (eTag != null)
            {
                var option = new DataLakeFileUploadOptions
                {
                    Conditions = new DataLakeRequestConditions { IfMatch = eTag }
                };

                result = await file.UploadAsync(fromStream, option, token);
                return result.Value.ETag;
            }

            result = await file.UploadAsync(fromStream, overwrite, token);
            return result.Value.ETag;
        }
    }
}