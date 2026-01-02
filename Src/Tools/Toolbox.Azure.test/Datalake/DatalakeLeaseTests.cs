//using Toolbox.Azure.test.Application;
//using Toolbox.Test.Store;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Azure.test.Datalake;

//public abstract class DatalakeLeaseBase
//{
//    protected readonly ScopeContext _context;
//    protected readonly FileStoreLeasedStandardTests _tests;

//    public DatalakeLeaseBase(ITestOutputHelper outputHelper)
//    {
//        _context = TestApplication.CreateScopeContext<DatalakeLeaseBase>(outputHelper);
//        _tests = new FileStoreLeasedStandardTests(() => TestApplication.GetDatalake("datastore-tests"), _context);
//    }
//}


//public class WhenWriteFile_AcquireLease_TestWriteAndReleaseTest : DatalakeLeaseBase
//{
//    public WhenWriteFile_AcquireLease_TestWriteAndReleaseTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

//    [Fact]
//    public Task WhenWriteFile_AcquireLease_TestWriteAndRelease()
//    {
//        return _tests.WhenWriteFile_AcquireLease_TestWriteAndRelease();
//    }
//}

//public class TwoClientTryGetLease_OneShouldFailTest : DatalakeLeaseBase
//{
//    public TwoClientTryGetLease_OneShouldFailTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

//    [Fact]
//    public Task TwoClientTryGetLease_OneShouldFail()
//    {
//        return _tests.TwoClientTryGetLease_OneShouldFail();
//    }
//}

//public class TwoClient_UsingScope_ShouldCoordinateTest : DatalakeLeaseBase
//{
//    public TwoClient_UsingScope_ShouldCoordinateTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

//    [Fact]
//    public Task TwoClient_UsingScope_ShouldCoordinate()
//    {
//        return _tests.TwoClient_UsingScope_ShouldCoordinate();
//    }
//}

//public class TwoClients_ExclusiveLock_SecondCannotAccessTest : DatalakeLeaseBase
//{
//    public TwoClients_ExclusiveLock_SecondCannotAccessTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

//    [Fact]
//    public Task TwoClients_ExclusiveLock_SecondCannotAccess()
//    {
//        return _tests.TwoClients_ExclusiveLock_SecondCannotAccess();
//    }
//}
