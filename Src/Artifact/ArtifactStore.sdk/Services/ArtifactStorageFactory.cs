using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Services
{
    public class ArtifactStorageFactory : IArtifactStorageFactory
    {
        private readonly IDataLakeStoreFactory _dataLakeStoreFactory;
        protected readonly ILoggerFactory _loggerFactory;

        public ArtifactStorageFactory(IDataLakeStoreFactory dataLakeStoreFactory, ILoggerFactory loggerFactory)
        {
            _dataLakeStoreFactory = dataLakeStoreFactory;
            _loggerFactory = loggerFactory;
        }

        public IArtifactStorage Create(string nameSpace)
        {
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            IDataLakeStore storage = _dataLakeStoreFactory.CreateStore(nameSpace)
                .VerifyNotNull($"Cannot create store for Namespace {nameSpace}");

            _dataLakeStoreFactory.TryGetValue(nameSpace, out DataLakeNamespace? dataLakeNamespace)
                .VerifyAssert(x => x == true, "Failed to get namespace properties");

            return new ArtifactStorage(storage, dataLakeNamespace!.PathRoot, _loggerFactory.CreateLogger<ArtifactStorage>());
        }

        public IDataLakeFileSystem? CreateFileSystem(string nameSpace) => _dataLakeStoreFactory.CreateFileSystem(nameSpace);
    }
}