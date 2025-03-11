using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.test.Application;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeLeaseTests
{
    public readonly ScopeContext _context;

    public DatalakeLeaseTests(ITestOutputHelper outputHelper)
    {
        _context = TestApplication.CreateScopeContext<DatalakeStoreTests>(outputHelper);
    }

    [Fact]
    public async Task AcquireLeaseAndRelease()
    {
        var datalakeClient1 = TestApplication.GetDatalake("datastore-tests");

        const string data = "this is a test";
        const string data2 = "updated this is a test";
        const string path = "acquireLease1.txt";

        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        var writeResult = await datalakeClient1.Write(path, dataBytes, true, _context);
        writeResult.IsOk().Should().BeTrue();

        var lease1 = await datalakeClient1.Acquire(path, TimeSpan.FromSeconds(30), _context);
        lease1.IsOk().Should().BeTrue();

        Option<DataETag> receive = await datalakeClient1.Read(path, _context);
        receive.IsOk().Should().BeTrue();
        Enumerable.SequenceEqual(dataBytes, receive.Return().Data).Should().BeTrue();

        dataBytes = Encoding.UTF8.GetBytes(data2);
        writeResult = await datalakeClient1.Write(path, dataBytes, true, _context);
        writeResult.IsOk().Should().BeTrue();

        var leaseResult = await lease1.Return().Release(_context);
        leaseResult.IsOk().Should().BeTrue();
    }
}
