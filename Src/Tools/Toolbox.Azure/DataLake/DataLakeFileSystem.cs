using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public class DatalakeFileSystem : IDatalakeFileSystem
    {
        private readonly DataLakeServiceClient _serviceClient;
        private readonly ILogger<DatalakeFileSystem> _logger;

        public DatalakeFileSystem(DatalakeStoreOption azureStoreOption, ILogger<DatalakeFileSystem> logger)
        {
            azureStoreOption.NotNull();
            logger.NotNull();

            _logger = logger;
            _serviceClient = azureStoreOption.CreateDataLakeServiceClient();
        }

        public async Task<IReadOnlyList<string>> List(CancellationToken token = default)
        {
            var list = new List<string>();

            await foreach (FileSystemItem file in _serviceClient.GetFileSystemsAsync(cancellationToken: token))
            {
                list.Add(file.Name);
            }

            return list;
        }

        public async Task Create(string name, CancellationToken token = default)
        {
            name.NotEmpty();
            bool created = false;

            CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"{nameof(Create)}: Create {name} file system");
                    await _serviceClient.CreateFileSystemAsync(name, cancellationToken: token);
                    created = true;
                    break;
                }
                catch (RequestFailedException ex) when (ex.ErrorCode != "ContainerBeingDeleted")
                {
                    throw;
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            while (!tokenSource.IsCancellationRequested)
            {
                IReadOnlyList<string> fileSystems = await List(token);
                if (fileSystems.SingleOrDefault(x => x == name) != null) return;

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            if (!created) throw new InvalidOperationException($"Could not create file system {name}");
        }

        public async Task Delete(string name, CancellationToken token = default)
        {
            name.NotEmpty();

            CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"{nameof(Delete)}: Delete {name} file system");
                    await _serviceClient.DeleteFileSystemAsync(name, cancellationToken: token);
                    return;
                }
                catch (RequestFailedException ex) when (ex.ErrorCode != "ContainerBeingDeleted")
                {
                    throw;
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        public async Task CreateIfNotExist(string name, CancellationToken token = default)
        {
            IReadOnlyList<string> fileSystemNames = await List(token);
            if (fileSystemNames.SingleOrDefault(x => x == name) != null) return;

            await Create(name, token);
        }

        public async Task DeleteIfExist(string name, CancellationToken token = default)
        {
            IReadOnlyList<string> fileSystemNames = await List(token);
            if (fileSystemNames.SingleOrDefault(x => x == name) == null) return;

            await Delete(name, token);
        }
    }
}