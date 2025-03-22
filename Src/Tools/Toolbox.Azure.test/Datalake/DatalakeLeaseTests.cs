using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Test.Store;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeLeaseTests
{
    private DatalakeLeaseStandardTests _tests;

    public DatalakeLeaseTests(ITestOutputHelper outputHelper)
    {
        _tests = new DatalakeLeaseStandardTests(() => TestApplication.GetDatalake("datastore-tests"), outputHelper);
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
