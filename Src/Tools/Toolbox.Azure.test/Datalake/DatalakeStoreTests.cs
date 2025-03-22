using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;
using Toolbox.Test.Store;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStoreTests
{
    public readonly IFileStore _dataLakeStore;
    public readonly ScopeContext _context;
    public readonly FileStoreFileStandardTests _tests;

    public DatalakeStoreTests(ITestOutputHelper outputHelper)
    {
        _dataLakeStore = TestApplication.GetDatalake("datastore-tests");
        _context = TestApplication.CreateScopeContext<DatalakeStoreTests>(outputHelper);
        _tests = new FileStoreFileStandardTests(_dataLakeStore, _context);
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