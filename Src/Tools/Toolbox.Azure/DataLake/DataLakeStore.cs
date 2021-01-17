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
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public class DataLakeStore : IDataLakeStore
    {
        private readonly DataLakeFileSystemClient _fileSystem;
        private readonly ILogger<DataLakeStore> _logger;
        private readonly DataLakeServiceClient _serviceClient;

        public DataLakeStore(DataLakeStoreOption azureStoreOption, ILogger<DataLakeStore> logger)
        {
            azureStoreOption.VerifyNotNull(nameof(azureStoreOption)).Verify();
            logger.VerifyNotNull(nameof(logger));

            _logger = logger;
            _serviceClient = azureStoreOption.CreateDataLakeServiceClient();

            // Get a reference to a file system (container)
            _fileSystem = _serviceClient.GetFileSystemClient(azureStoreOption.ContainerName);
        }

        public async Task Append(string path, byte[] data, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));
            data
                .VerifyNotNull(nameof(data))
                .VerifyAssert(x => x.Length > 0, $"{nameof(data)} length must be greater then 0");

            _logger.LogTrace($"{nameof(Write)} to {path}");
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
                await Write(path, data, true, token);
            }
            catch (TaskCanceledException) { }
        }

        public async Task<bool> Delete(string path, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));

            _logger.LogTrace($"{nameof(Delete)} deleting {path}");

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.DeleteIfExistsAsync(cancellationToken: token);

            return response.Value;
        }

        public async Task DeleteDirectory(string path, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));

            _logger.LogTrace($"{nameof(DeleteDirectory)} {path}");

            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            await directoryClient.DeleteAsync(cancellationToken: token);
        }

        public async Task Download(string path, Stream toStream, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));
            toStream.VerifyNotNull(nameof(toStream));

            _logger.LogTrace($"{nameof(Download)} downloading {path} to stream");

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            await file.ReadToAsync(toStream, cancellationToken: token);
        }

        public async Task<bool> Exist(string path, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.ExistsAsync(token);
            return response.Value;
        }

        public async Task<DatalakePathProperties> GetPathProperties(string path, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            return (await file.GetPropertiesAsync(cancellationToken: token))
                .Value
                .ConvertTo();
        }

        public async Task<byte[]> Read(string path, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<FileDownloadInfo> response = await file.ReadAsync(token);

            _logger.LogTrace($"{nameof(Read)} from {path}");

            using MemoryStream memory = new MemoryStream();
            await response.Value.Content.CopyToAsync(memory);

            return memory.ToArray();
        }

        public async Task<IReadOnlyList<DataLakePathItem>> Search(string? path, Func<DataLakePathItem, bool> filter, bool recursive, CancellationToken token)
        {
            var list = new List<DataLakePathItem>();

            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(path, recursive, cancellationToken: token))
            {
                DataLakePathItem datalakePathItem = pathItem.ConvertTo();

                if (filter(datalakePathItem)) list.Add(datalakePathItem);
            }

            return list;
        }

        public async Task Upload(Stream fromStream, string toPath, bool force, CancellationToken token)
        {
            fromStream.VerifyNotNull(nameof(fromStream));
            toPath.VerifyNotEmpty(nameof(toPath));

            _logger.LogTrace($"{nameof(Upload)} from stream to {toPath}");

            DataLakeFileClient file = _fileSystem.GetFileClient(toPath);
            await file.UploadAsync(fromStream, force, token);
        }
        public async Task Write(string path, byte[] data, bool force, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));
            data
                .VerifyNotNull(nameof(data))
                .VerifyAssert(x => x.Length > 0, $"{nameof(data)} length must be greater then 0");

            _logger.LogTrace($"{nameof(Write)} to {path}");
            using var memoryBuffer = new MemoryStream(data.ToArray());

            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            await file.UploadAsync(memoryBuffer, force, token);
        }
    }
}