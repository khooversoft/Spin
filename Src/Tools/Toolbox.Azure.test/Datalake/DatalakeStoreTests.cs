using Toolbox.Azure.test.Application;
using Toolbox.Store;
using Toolbox.Test.Store;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStoreTests
{
    public readonly IFileStore _dataLakeStore;
    public readonly ScopeContext _context;
    public readonly FileStoreFileAccessStandardTests _tests;

    public DatalakeStoreTests(ITestOutputHelper outputHelper)
    {
        _dataLakeStore = TestApplication.GetDatalake("datastore-tests");
        _context = TestApplication.CreateScopeContext<DatalakeStoreTests>(outputHelper);
        _tests = new FileStoreFileAccessStandardTests(_dataLakeStore, _context);
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