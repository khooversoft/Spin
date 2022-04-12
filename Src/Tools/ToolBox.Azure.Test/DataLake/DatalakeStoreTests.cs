using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly DatalakeStoreOption _testOption;
        private readonly ILoggerFactory _loggerFactory = new TestLoggerFactory();

        public DatalakeStoreTests() => _testOption = TestHost.Default.GetDatalakeStoreOption() with { ContainerName = "adls-store-test" };

        [Fact]
        public async Task GivenData_WhenSaved_ShouldWork()
        {
            const string data = "this is a test";
            const string path = "testString.txt";

            await InitializeFileSystem();
            IDatalakeStore dataLakeStore = new DatalakeStore(_testOption, _loggerFactory.CreateLogger<DatalakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            await dataLakeStore.Write(path, dataBytes, true);

            byte[]? receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();

            Enumerable.SequenceEqual(dataBytes, receive!).Should().BeTrue();

            DatalakePathProperties? pathProperties = await dataLakeStore.GetPathProperties(path);
            pathProperties.Should().NotBeNull();
            pathProperties!.ETag.Should().NotBeNull();

            (await dataLakeStore.Exist(path)).Should().BeTrue();
            (await dataLakeStore.GetPathProperties(path)).Should().NotBeNull();

            await dataLakeStore.Delete(path);
            (await dataLakeStore.Exist(path)).Should().BeFalse();

            IReadOnlyList<DatalakePathItem> list = await dataLakeStore.Search(null!);
            list.Should().NotBeNull();
        }

        [Fact]
        public async Task GivenNewFile_WhenAppended_ShouldWork()
        {
            const string data1 = "this is a test - first line(n)";
            const string data2 = "*** second line ****";
            const string path = "testStringAppend1.txt";

            await InitializeFileSystem();
            IDatalakeStore dataLakeStore = new DatalakeStore(_testOption, _loggerFactory.CreateLogger<DatalakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
            await dataLakeStore.Write(path, dataBytes, true);

            byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
            await dataLakeStore.Append(path, appendDataBytes);

            byte[]? receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();

            Enumerable.SequenceEqual(dataBytes.Concat(appendDataBytes), receive!).Should().BeTrue();

            await dataLakeStore.Delete(path);
        }

        [Fact]
        public async Task GivenExistingFile_WhenAppended_ShouldWork()
        {
            string data1 = "this is a test number 2 - first line(n)" + Environment.NewLine;
            const string data2 = "*** second line of number 2 ****";
            const string path = "testStringAppend2.txt";

            await InitializeFileSystem();
            IDatalakeStore dataLakeStore = new DatalakeStore(_testOption, _loggerFactory.CreateLogger<DatalakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
            await dataLakeStore.Append(path, dataBytes);

            byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
            await dataLakeStore.Append(path, appendDataBytes);

            byte[]? receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();

            Enumerable.SequenceEqual(dataBytes.Concat(appendDataBytes), receive!).Should().BeTrue();

            await dataLakeStore.Delete(path);
        }

        [Fact]
        public async Task GivenFile_WhenSaved_ShouldWork()
        {
            const string path = "base/folder/Test.json";

            await InitializeFileSystem();
            IDatalakeStore dataLakeStore = new DatalakeStore(_testOption, _loggerFactory.CreateLogger<DatalakeStore>());

            string originalFilePath = TestHost.WriteResourceToFile(Path.GetFileName(path));
            originalFilePath.Should().NotBeNullOrEmpty();

            using (Stream readFile = new FileStream(originalFilePath, FileMode.Open))
            {
                await dataLakeStore.Write(readFile, path, true);
            }

            string downloadFilePath = Path.Combine(Path.GetDirectoryName(originalFilePath)!, Path.GetFileName(path) + ".downloaded");

            using (Stream writeFile = new FileStream(downloadFilePath, FileMode.Create))
            {
                await dataLakeStore.Read(path, writeFile);
            }

            byte[] originalFileHash = GetFileHash(originalFilePath);
            byte[] downloadFileHash = GetFileHash(downloadFilePath);

            Enumerable.SequenceEqual(originalFileHash, downloadFileHash).Should().BeTrue();

            DatalakePathProperties? pathProperties = await dataLakeStore.GetPathProperties(path);
            pathProperties.Should().NotBeNull();
            pathProperties!.ETag.Should().NotBeNull();

            (await dataLakeStore.Exist(path)).Should().BeTrue();
            (await dataLakeStore.GetPathProperties(path)).Should().NotBeNull();

            await dataLakeStore.Delete(path);
            (await dataLakeStore.Exist(path)).Should().BeFalse();
        }

        //[Trait("Category", "Unit")]
        [Fact]
        public async Task GivenFiles_WhenSearched_ReturnsCorrectly()
        {
            const string path = "Test.json";

            await InitializeFileSystem();
            IDatalakeStore dataLakeStore = new DatalakeStore(_testOption, _loggerFactory.CreateLogger<DatalakeStore>());

            await ClearContainer(dataLakeStore);

            IReadOnlyList<DatalakePathItem> verifyList = await dataLakeStore.Search(null!);
            verifyList.Should().NotBeNull();
            verifyList.Count.Should().Be(0);

            string originalFilePath = TestHost.WriteResourceToFile(Path.GetFileName(path));
            originalFilePath.Should().NotBeNullOrEmpty();

            string[] fileLists = new[]
            {
                "test1.json",
                "test2.json",
                "data/test3.json",
                "data/test4.json",
                "data2/test5.json"
            };

            foreach (var filePath in fileLists)
            {
                using (Stream readFile = new FileStream(originalFilePath, FileMode.Open))
                {
                    await dataLakeStore.Write(readFile, filePath, true);
                }
            }

            IReadOnlyList<DatalakePathItem> subSearchList = await dataLakeStore.Search(new QueryParameter { Filter = "data" });
            subSearchList.Should().NotBeNull();
            subSearchList.Count.Should().Be(fileLists.Where(x => x.StartsWith("data/")).Count());

            IReadOnlyList<DatalakePathItem> searchList = await dataLakeStore.Search(null!);
            searchList.Should().NotBeNull();
            searchList.Where(x => x.IsDirectory == false).Count().Should().Be(2);
            searchList.Where(x => x.IsDirectory == true).Count().Should().Be(2);

            await ClearContainer(dataLakeStore);

            searchList = await dataLakeStore.Search(null!);
            searchList.Should().NotBeNull();
            searchList.Count.Should().Be(0);
        }

        private async Task InitializeFileSystem()
        {
            IDatalakeFileSystem management = new DatalakeFileSystem(_testOption, _loggerFactory.CreateLogger<DatalakeFileSystem>());
            await management.CreateIfNotExist(_testOption.ContainerName);
        }

        private static byte[] GetFileHash(string file)
        {
            using Stream read = new FileStream(file, FileMode.Open);
            return MD5.Create().ComputeHash(read);
        }

        private async Task ClearContainer(IDatalakeStore dataLakeStore)
        {
            IReadOnlyList<DatalakePathItem> list = await dataLakeStore.Search(null!);
            list.Should().NotBeNull();

            foreach (var fileItem in list.Where(x => x.IsDirectory == true))
            {
                await dataLakeStore.DeleteDirectory(fileItem.Name!);
            }

            foreach (var fileItem in list.Where(x => x.IsDirectory == false))
            {
                await dataLakeStore.Delete(fileItem.Name!);
            }
        }
    }
}
