using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.Memory;

public class MemoryStoreLeaseTests
{
    private readonly IServiceProvider _serviceProvider;
    public readonly IFileStore _fileStore;
    public readonly ScopeContext _context;
    public readonly FileStoreLeasedStandardTests _tests;

    public MemoryStoreLeaseTests(ITestOutputHelper outputHelper)
    {
        _serviceProvider = TestApplication.CreateServiceProvider(outputHelper);
        _fileStore = _serviceProvider.GetRequiredService<IFileStore>();
        _context = new ScopeContext(_serviceProvider.GetRequiredService<ILogger<MemoryStoreFileAccessTests>>());
        _tests = new FileStoreLeasedStandardTests(() => _fileStore, _context);
    }

    [Fact]
    public Task WhenWriteFile_AcquireLease_TestWriteAndRelease()
    {
        return _tests.WhenWriteFile_AcquireLease_TestWriteAndRelease();
    }

    [Fact]
    public Task TwoClientTryGetLease_OneShouldFail()
    {
        return _tests.TwoClientTryGetLease_OneShouldFail();
    }

    [Fact]
    public Task TwoClient_UsingScope_ShouldCoordinate()
    {
        return _tests.TwoClient_UsingScope_ShouldCoordinate();
    }

    [Fact]
    public Task TwoClients_ExclusiveLock_SecondCannotAccess()
    {
        return _tests.TwoClients_ExclusiveLock_SecondCannotAccess();
    }
}