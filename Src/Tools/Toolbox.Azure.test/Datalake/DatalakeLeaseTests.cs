using Toolbox.Azure.test.Application;
using Toolbox.Test.Store;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeLeaseTests
{
    private readonly ScopeContext _context;
    private FileStoreLeasedStandardTests _tests;

    public DatalakeLeaseTests(ITestOutputHelper outputHelper)
    {
        _context = TestApplication.CreateScopeContext<DatalakeLeaseTests>(outputHelper);
        _tests = new FileStoreLeasedStandardTests(() => TestApplication.GetDatalake("datastore-tests"), _context);
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
