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
        private readonly DataLakeStoreOption _testOption;
        private readonly ILoggerFactory _loggerFactory = new TestLoggerFactory();

        public DatalakeStoreEtagTests() => _testOption = new TestOptionBuilder().Build() with { ContainerName = "adls-etag-test" };

        [Fact]
        public async Task GivenData_WhenSaved_ShouldMatchEtag()
        {
            const string data = "this is a test";
            const string path = "testStringEtag.txt";

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

            receive = await dataLakeStore.Read(path, CancellationToken.None);
            receive.Should().NotBeNull();

            DatalakePathProperties? pathProperties2ndRead = await dataLakeStore.GetPathProperties(path, CancellationToken.None);
            pathProperties2ndRead.Should().NotBeNull();
            pathProperties2ndRead!.ETag.Should().NotBeNullOrEmpty();

            pathProperties.ETag.Should().Be(pathProperties2ndRead.ETag);

            await dataLakeStore.Delete(path, CancellationToken.None);
        }

        [Fact]
        public async Task GivenData_WhenSavedAndWritten_ShouldNotMatchEtag()
        {
            const string data = "this is a test";
            const string path = "testStringEtag.txt";

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

            await dataLakeStore.Write(path, dataBytes, true, CancellationToken.None);
            receive = await dataLakeStore.Read(path, CancellationToken.None);
            receive.Should().NotBeNull();

            DatalakePathProperties? pathProperties2ndRead = await dataLakeStore.GetPathProperties(path, CancellationToken.None);
            pathProperties2ndRead.Should().NotBeNull();
            pathProperties2ndRead!.ETag.Should().NotBeNullOrEmpty();

            pathProperties.ETag.Should().NotBe(pathProperties2ndRead.ETag);

            await dataLakeStore.Delete(path, CancellationToken.None);
        }

        [Fact]
        public async Task GivenData_WhenSavedAndUpdate_ShouldNotMatchEtag()
        {
            const string data = "this is a test";
            const string data2 = "this is a test2";
            const string path = "testStringEtag.txt";

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

            byte[] data2Bytes = Encoding.UTF8.GetBytes(data2);
            await dataLakeStore.Write(path, data2Bytes, true, CancellationToken.None);

            receive = await dataLakeStore.Read(path, CancellationToken.None);
            receive.Should().NotBeNull();

            DatalakePathProperties? pathProperties2ndRead = await dataLakeStore.GetPathProperties(path, CancellationToken.None);
            pathProperties2ndRead.Should().NotBeNull();
            pathProperties2ndRead!.ETag.Should().NotBeNullOrEmpty();

            pathProperties.ETag.Should().NotBe(pathProperties2ndRead.ETag);

            await dataLakeStore.Delete(path, CancellationToken.None);
            (await dataLakeStore.Exist(path, CancellationToken.None)).Should().BeFalse();
        }

        private async Task InitializeFileSystem()
        {
            IDataLakeFileSystem management = new DataLakeFileSystem(_testOption, _loggerFactory.CreateLogger<DataLakeFileSystem>());
            await management.CreateIfNotExist(_testOption.ContainerName, CancellationToken.None);
        }
    }
}
