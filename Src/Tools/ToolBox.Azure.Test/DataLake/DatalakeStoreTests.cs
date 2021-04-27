using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using ToolBox.Azure.Test.Application;
using Xunit;

namespace ToolBox.Azure.Test.DataLake
{
    public class DatalakeStoreTests
    {
        private readonly DataLakeStoreOption _testOption;
        private readonly ILoggerFactory _loggerFactory = new TestLoggerFactory();

        public DatalakeStoreTests() => _testOption = new TestOptionBuilder().Build() with { ContainerName = "adls-store-test" };

        [Fact]
        public async Task GivenData_WhenSaved_ShouldWork()
        {
            const string data = "this is a test";
            const string path = "testString.txt";

            await InitializeFileSystem();
            IDataLakeStore dataLakeStore = new DataLakeStore(_testOption, _loggerFactory.CreateLogger<DataLakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            await dataLakeStore.Write(path, dataBytes, true, CancellationToken.None);

            byte[] receive = await dataLakeStore.Read(path, CancellationToken.None);
            receive.Should().NotBeNull();

            Enumerable.SequenceEqual(dataBytes, receive).Should().BeTrue();

            DatalakePathProperties? pathProperties = await dataLakeStore.GetPathProperties(path, CancellationToken.None);
            pathProperties.Should().NotBeNull();
            pathProperties!.ETag.Should().NotBeNullOrEmpty();

            (await dataLakeStore.Exist(path, CancellationToken.None)).Should().BeTrue();
            (await dataLakeStore.GetPathProperties(path, CancellationToken.None)).Should().NotBeNull();

            await dataLakeStore.Delete(path, CancellationToken.None);
            (await dataLakeStore.Exist(path, CancellationToken.None)).Should().BeFalse();

            IReadOnlyList<DataLakePathItem> list = await dataLakeStore.Search(null!, x => true, true, CancellationToken.None);
            list.Should().NotBeNull();
        }

        [Fact]
        public async Task GivenNewFile_WhenAppended_ShouldWork()
        {
            const string data1 = "this is a test - first line(n)";
            const string data2 = "*** second line ****";
            const string path = "testStringAppend1.txt";

            await InitializeFileSystem();
            IDataLakeStore dataLakeStore = new DataLakeStore(_testOption, _loggerFactory.CreateLogger<DataLakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
            await dataLakeStore.Write(path, dataBytes, true, CancellationToken.None);

            byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
            await dataLakeStore.Append(path, appendDataBytes, CancellationToken.None);

            byte[] receive = await dataLakeStore.Read(path, CancellationToken.None);
            receive.Should().NotBeNull();

            Enumerable.SequenceEqual(dataBytes.Concat(appendDataBytes), receive).Should().BeTrue();

            await dataLakeStore.Delete(path, CancellationToken.None);
        }

        [Fact]
        public async Task GivenExistingFile_WhenAppended_ShouldWork()
        {
            string data1 = "this is a test number 2 - first line(n)" + Environment.NewLine;
            const string data2 = "*** second line of number 2 ****";
            const string path = "testStringAppend2.txt";

            await InitializeFileSystem();
            IDataLakeStore dataLakeStore = new DataLakeStore(_testOption, _loggerFactory.CreateLogger<DataLakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
            await dataLakeStore.Append(path, dataBytes, CancellationToken.None);

            byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
            await dataLakeStore.Append(path, appendDataBytes, CancellationToken.None);

            byte[] receive = await dataLakeStore.Read(path, CancellationToken.None);
            receive.Should().NotBeNull();

            Enumerable.SequenceEqual(dataBytes.Concat(appendDataBytes), receive).Should().BeTrue();

            await dataLakeStore.Delete(path, CancellationToken.None);
        }

        [Fact]
        public async Task GivenFile_WhenSaved_ShouldWork()
        {
            const string path = "base/folder/Test.json";

            await InitializeFileSystem();
            IDataLakeStore dataLakeStore = new DataLakeStore(_testOption, _loggerFactory.CreateLogger<DataLakeStore>());

            string originalFilePath = TestOptionBuilder.WriteResourceToFile(Path.GetFileName(path));
            originalFilePath.Should().NotBeNullOrEmpty();

            using (Stream readFile = new FileStream(originalFilePath, FileMode.Open))
            {
                await dataLakeStore.Upload(readFile, path, true, CancellationToken.None);
            }

            string downloadFilePath = Path.Combine(Path.GetDirectoryName(originalFilePath)!, Path.GetFileName(path) + ".downloaded");

            using (Stream writeFile = new FileStream(downloadFilePath, FileMode.Create))
            {
                await dataLakeStore.Download(path, writeFile, CancellationToken.None);
            }

            byte[] originalFileHash = GetFileHash(originalFilePath);
            byte[] downloadFileHash = GetFileHash(downloadFilePath);

            Enumerable.SequenceEqual(originalFileHash, downloadFileHash).Should().BeTrue();

            DatalakePathProperties? pathProperties = await dataLakeStore.GetPathProperties(path, CancellationToken.None);
            pathProperties.Should().NotBeNull();
            pathProperties!.ETag.Should().NotBeNullOrEmpty();

            (await dataLakeStore.Exist(path, CancellationToken.None)).Should().BeTrue();
            (await dataLakeStore.GetPathProperties(path, CancellationToken.None)).Should().NotBeNull();

            await dataLakeStore.Delete(path, CancellationToken.None);
            (await dataLakeStore.Exist(path, CancellationToken.None)).Should().BeFalse();
        }

        [Trait("Category", "Unit")]
        [Fact]
        public async Task GivenFiles_WhenSearched_ReturnsCorrectly()
        {
            const string path = "Test.json";

            await InitializeFileSystem();
            IDataLakeStore dataLakeStore = new DataLakeStore(_testOption, _loggerFactory.CreateLogger<DataLakeStore>());

            await ClearContainer(dataLakeStore);

            IReadOnlyList<DataLakePathItem> verifyList = await dataLakeStore.Search(null!, x => true, true, CancellationToken.None);
            verifyList.Should().NotBeNull();
            verifyList.Count.Should().Be(0);

            string originalFilePath = TestOptionBuilder.WriteResourceToFile(Path.GetFileName(path));
            originalFilePath.Should().NotBeNullOrEmpty();

            string[] fileLists = new[]
            {
                "test1.json",
                "test2.json",
                "data/test3.json",
                "data/test4.json",
                "data2/test5.json"
            };

            int folderCount = fileLists
                .Select(x => (x, vectors: x.Split("/")))
                .Where(x => x.vectors.Length > 1)
                .Select(x => x.vectors[0])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            foreach (var filePath in fileLists)
            {
                using (Stream readFile = new FileStream(originalFilePath, FileMode.Open))
                {
                    await dataLakeStore.Upload(readFile, filePath, true, CancellationToken.None);
                }
            }

            IReadOnlyList<DataLakePathItem> subSearchList = await dataLakeStore.Search(new QueryParameter { Filter = "data" }, x => true, true, CancellationToken.None);
            subSearchList.Should().NotBeNull();
            subSearchList.Count.Should().Be(fileLists.Where(x => x.StartsWith("data/")).Count());

            IReadOnlyList<DataLakePathItem> searchList = await dataLakeStore.Search(null!, x => true, true, CancellationToken.None);
            searchList.Should().NotBeNull();
            searchList.Where(x => x.IsDirectory == false).Count().Should().Be(fileLists.Length);
            searchList.Where(x => x.IsDirectory == true).Count().Should().Be(folderCount);

            await ClearContainer(dataLakeStore);

            searchList = await dataLakeStore.Search(null!, x => true, true, CancellationToken.None);
            searchList.Should().NotBeNull();
            searchList.Count.Should().Be(0);
        }

        private async Task InitializeFileSystem()
        {
            IDataLakeFileSystem management = new DataLakeFileSystem(_testOption, _loggerFactory.CreateLogger<DataLakeFileSystem>());
            await management.CreateIfNotExist(_testOption.ContainerName, CancellationToken.None);
        }

        private static byte[] GetFileHash(string file)
        {
            using Stream read = new FileStream(file, FileMode.Open);
            return MD5.Create().ComputeHash(read);
        }

        private async Task ClearContainer(IDataLakeStore dataLakeStore)
        {
            IReadOnlyList<DataLakePathItem> list = await dataLakeStore.Search(null!, x => true, false, CancellationToken.None);
            list.Should().NotBeNull();

            foreach (var fileItem in list.Where(x => x.IsDirectory == true))
            {
                await dataLakeStore.DeleteDirectory(fileItem.Name!, CancellationToken.None);
            }

            foreach (var fileItem in list.Where(x => x.IsDirectory == false))
            {
                await dataLakeStore.Delete(fileItem.Name!, CancellationToken.None);
            }
        }
    }
}
