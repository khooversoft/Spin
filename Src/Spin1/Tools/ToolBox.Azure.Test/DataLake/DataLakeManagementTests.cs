using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using ToolBox.Azure.Test.Application;
using Xunit;

namespace ToolBox.Azure.Test.DataLake
{
    public class DataLakeManagementTests
    {
        private readonly DatalakeStoreOption _testOption;
        private readonly ILoggerFactory _loggerFactory = new TestLoggerFactory();

        public DataLakeManagementTests() => _testOption = TestHost.Default.GetDatalakeStoreOption();

        [Fact]
        public async Task GivenFileSystem_WhenExist_SearchDoesReturn()
        {
            var option = _testOption with { ContainerName = _testOption.ContainerName + 1 };

            IDatalakeFileSystem management = new DatalakeFileSystem(option, _loggerFactory.CreateLogger<DatalakeFileSystem>());
            await management.DeleteIfExist(option.ContainerName, CancellationToken.None);

            await management.Create(option.ContainerName, CancellationToken.None);

            IReadOnlyList<string> list = await management.List(CancellationToken.None);
            list.Count(x => x == option.ContainerName).Should().Be(1);

            await management.Delete(option.ContainerName, CancellationToken.None);
        }

        [Fact]
        public async Task GivenFileSystem_WhenNotExist_SearchDoesNotReturn()
        {
            var option = _testOption with { ContainerName = _testOption.ContainerName + 2 };

            IDatalakeFileSystem management = new DatalakeFileSystem(option, _loggerFactory.CreateLogger<DatalakeFileSystem>());
            await management.DeleteIfExist(option.ContainerName, CancellationToken.None);

            IReadOnlyList<string> list = await management.List(CancellationToken.None);
            list.Count(x => x == option.ContainerName).Should().Be(0);
        }

        [Fact]
        public async Task GivenFileSystem_WhenNotExist_CreateShouldExist()
        {
            var option = _testOption with { ContainerName = _testOption.ContainerName + 3 };

            IDatalakeFileSystem management = new DatalakeFileSystem(option, _loggerFactory.CreateLogger<DatalakeFileSystem>());
            await management.DeleteIfExist(option.ContainerName, CancellationToken.None);

            await management.CreateIfNotExist(option.ContainerName, CancellationToken.None);

            IReadOnlyList<string> list = await management.List(CancellationToken.None);
            list.Count(x => x == option.ContainerName).Should().Be(1);

            await management.Delete(option.ContainerName, CancellationToken.None);
        }
    }
}