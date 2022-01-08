using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using ToolBox.Azure.Test.Application;
using Xunit;

namespace ToolBox.Azure.Test.DataLake
{
    public class DatalakeStoreEtagTests
    {
        private readonly DatalakeStoreOption _testOption;
        private readonly ILoggerFactory _loggerFactory = new TestLoggerFactory();

        public DatalakeStoreEtagTests() => _testOption = new TestOptionBuilder().Build() with { ContainerName = "adls-etag-test" };

        [Fact]
        public async Task GivenData_WhenSaved_ShouldMatchEtag()
        {
            const string data = "this is a test";
            const string path = "testStringEtag.txt";

            await InitializeFileSystem();
            IDatalakeStore dataLakeStore = new DatalakeStore(_testOption, _loggerFactory.CreateLogger<DatalakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            await dataLakeStore.Write(path, dataBytes, true, token: CancellationToken.None);

            byte[]? receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();
            Enumerable.SequenceEqual(dataBytes, receive!).Should().BeTrue();

            DatalakePathProperties? pathProperties = await dataLakeStore.GetPathProperties(path);
            pathProperties.Should().NotBeNull();
            pathProperties!.ETag.Should().NotBeNull();

            receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();

            DatalakePathProperties? pathProperties2ndRead = await dataLakeStore.GetPathProperties(path);
            pathProperties2ndRead.Should().NotBeNull();
            pathProperties2ndRead!.ETag.Should().NotBeNull();

            pathProperties.ETag.Should().Be(pathProperties2ndRead.ETag);

            await dataLakeStore.Delete(path);
        }

        [Fact]
        public async Task GivenData_WhenSavedAndWritten_ShouldNotMatchEtag()
        {
            const string data = "this is a test";
            const string path = "testStringEtag.txt";

            await InitializeFileSystem();
            IDatalakeStore dataLakeStore = new DatalakeStore(_testOption, _loggerFactory.CreateLogger<DatalakeStore>());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            await dataLakeStore.Write(path, dataBytes, true, token: CancellationToken.None);

            byte[]? receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();
            Enumerable.SequenceEqual(dataBytes, receive!).Should().BeTrue();

            DatalakePathProperties? pathProperties = await dataLakeStore.GetPathProperties(path);
            pathProperties.Should().NotBeNull();
            pathProperties!.ETag.Should().NotBeNull();

            await dataLakeStore.Write(path, dataBytes, true, token: CancellationToken.None);
            receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();

            DatalakePathProperties? pathProperties2ndRead = await dataLakeStore.GetPathProperties(path);
            pathProperties2ndRead.Should().NotBeNull();
            pathProperties2ndRead!.ETag.Should().NotBeNull();

            pathProperties.ETag.Should().NotBe(pathProperties2ndRead.ETag);

            await dataLakeStore.Delete(path, token: CancellationToken.None);
        }

        [Fact]
        public async Task GivenData_WhenSavedAndUpdate_ShouldNotMatchEtag()
        {
            const string data = "this is a test";
            const string data2 = "this is a test2";
            const string path = "testStringEtag.txt";

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

            byte[] data2Bytes = Encoding.UTF8.GetBytes(data2);
            await dataLakeStore.Write(path, data2Bytes, true, token: CancellationToken.None);

            receive = await dataLakeStore.Read(path);
            receive.Should().NotBeNull();

            DatalakePathProperties? pathProperties2ndRead = await dataLakeStore.GetPathProperties(path);
            pathProperties2ndRead.Should().NotBeNull();
            pathProperties2ndRead!.ETag.Should().NotBeNull();

            pathProperties.ETag.Should().NotBe(pathProperties2ndRead.ETag);

            await dataLakeStore.Delete(path);
            (await dataLakeStore.Exist(path)).Should().BeFalse();
        }

        private async Task InitializeFileSystem()
        {
            IDatalakeFileSystem management = new DatalakeFileSystem(_testOption, _loggerFactory.CreateLogger<DatalakeFileSystem>());
            await management.CreateIfNotExist(_testOption.ContainerName);
        }
    }
}
