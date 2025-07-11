using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.Memory;

public class MemoryStoreTests
{
    private readonly IServiceProvider _serviceProvider;
    public readonly IFileStore _fileStore;
    public readonly ScopeContext _context;
    public readonly FileStoreFileAccessStandardTests _tests;

    public MemoryStoreTests(ITestOutputHelper outputHelper)
    {
        _serviceProvider = TestApplication.CreateServiceProvider(outputHelper);
        _fileStore = _serviceProvider.GetRequiredService<IFileStore>();
        _context = new ScopeContext(_serviceProvider.GetRequiredService<ILogger<MemoryStoreTests>>());
        _tests = new FileStoreFileAccessStandardTests(_fileStore, _context);
    }

    [Fact]
    public async Task GivenData_WhenSaved_ShouldWork()
    {
        await _tests.GivenData_WhenSaved_ShouldWork();
    }

    [Fact]
    public async Task GivenNewFile_WhenAppended_ShouldCreateThenAppend()
    {
        await _tests.GivenNewFile_WhenAppended_ShouldCreateThenAppend();
    }

    [Fact]
    public async Task GivenNewFileCreated_WhenAppended_ShouldWork()
    {
        await _tests.GivenNewFileCreated_WhenAppended_ShouldWork();
    }

    [Fact]
    public async Task GivenExistingFile_WhenAppended_ShouldWork()
    {
        await _tests.GivenExistingFile_WhenAppended_ShouldWork();
    }

    [Fact]
    public async Task GivenFiles_WhenSearched_ReturnsCorrectly()
    {
        await _tests.GivenFiles_WhenSearched_ReturnsCorrectly();
    }
}
