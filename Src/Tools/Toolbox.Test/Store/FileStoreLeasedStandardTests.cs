using System.Text;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Store;

public class FileStoreLeasedStandardTests
{
    private readonly ScopeContext _context;
    private readonly Func<IFileStore> _getFileStore;

    public FileStoreLeasedStandardTests(Func<IFileStore> getFileStore, ScopeContext context)
    {
        _getFileStore = getFileStore.NotNull();
        _context = context;
    }

    public async Task WhenWriteFile_AcquireLease_TestWriteAndRelease()
    {
        IFileStore fileStore1 = _getFileStore();

        const string data = "this is a test";
        const string data2 = "updated this is a test";
        const string data3 = "Third test";
        const string path = "acquireLease1.txt";

        var fileAccess = fileStore1.File(path);

        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        var writeResult = await fileAccess.Set(dataBytes, _context);
        writeResult.IsOk().Should().BeTrue();

        var lease1Option = await fileAccess.Acquire(TimeSpan.FromSeconds(60), _context);
        lease1Option.IsOk().Should().BeTrue();
        var lease1 = lease1Option.Return();

        Option<DataETag> receive = await fileAccess.Get(_context);
        receive.IsOk().Should().BeTrue();
        Enumerable.SequenceEqual(dataBytes, receive.Return().Data).Should().BeTrue();

        dataBytes = Encoding.UTF8.GetBytes(data2);
        writeResult = await lease1.Set(dataBytes, _context);
        writeResult.IsOk().Should().BeTrue(writeResult.ToString());

        var leaseResult = await lease1.Release(_context);
        leaseResult.IsOk().Should().BeTrue(leaseResult.ToString());

        dataBytes = Encoding.UTF8.GetBytes(data3);
        writeResult = await fileAccess.Set(dataBytes, _context);
        writeResult.IsOk().Should().BeTrue(writeResult.ToString());

        receive = await fileAccess.Get(_context);
        receive.IsOk().Should().BeTrue();
        Enumerable.SequenceEqual(dataBytes, receive.Return().Data).Should().BeTrue();
    }

    public async Task TwoClientTryGetLease_OneShouldFail()
    {
        IFileStore fileStore1 = _getFileStore();
        IFileStore fileStore2 = _getFileStore();

        const string data = "this is a test";
        const string data2 = "this is a test";
        const string data3 = "this is a test";
        const string path = "acquireLease2.txt";

        var fileAccess1 = fileStore1.File(path);
        var fileAccess2 = fileStore2.File(path);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] dataBytes2 = Encoding.UTF8.GetBytes(data2);
        byte[] dataBytes3 = Encoding.UTF8.GetBytes(data3);

        var writeResult = (await fileAccess1.Set(dataBytes, _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();

        await using var lease1 = (await fileAccess1.Acquire(TimeSpan.FromSeconds(30), _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();

        var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(125));
        var context = _context.With(cancelTokenSource.Token);

        var lease2Option = await fileAccess1.Acquire(TimeSpan.FromSeconds(60), context);
        lease2Option.IsError().Should().BeTrue();

        (await fileAccess2.Set(dataBytes, _context)).Assert(x => x.IsConflict(), x => x.ToString());
        (await fileAccess1.Set(dataBytes3, _context)).Assert(x => x.IsError(), _ => "Should fail");

        (await lease1.Release(_context)).Assert(x => x.IsOk(), x => x.ToString());

        dataBytes = Encoding.UTF8.GetBytes(data3);
        (await fileAccess1.Set(dataBytes, _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();

        var receive = (await fileAccess1.Get(_context)).Assert(x => x.IsOk(), x => x.ToString()).Return();
        Enumerable.SequenceEqual(dataBytes, receive.Data).Should().BeTrue();
    }

    public async Task TwoClient_UsingScope_ShouldCoordinate()
    {
        IFileStore fileStore1 = _getFileStore();
        IFileStore fileStore2 = _getFileStore();

        const string data = "this is a test";
        const string data2 = "this is a test";
        const string data3 = "this is a test";
        const string path = "acquireLease3.txt";

        var fileAccess1 = fileStore1.File(path);

        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        var writeResult = (await fileAccess1.Set(dataBytes, _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();

        var lease1 = (await fileAccess1.Acquire(TimeSpan.FromSeconds(30), _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();
        await using (lease1)
        {
            dataBytes = Encoding.UTF8.GetBytes(data2);
            (await lease1.Set(dataBytes, _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();

            (await lease1.Get(_context))
                .Assert(x => x.IsOk(), x => x.ToString())
                .Return().Action(x => Enumerable.SequenceEqual(dataBytes, x.Data).Should().BeTrue());
        }

        var fileAccess2 = fileStore2.File(path);
        var lease2 = (await fileAccess2.Acquire(TimeSpan.FromSeconds(30), _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();
        await using (lease2)
        {
            dataBytes = Encoding.UTF8.GetBytes(data3);
            (await lease2.Set(dataBytes, _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();

            (await lease2.Get(_context))
                .Assert(x => x.IsOk(), x => x.ToString())
                .Return().Action(x => Enumerable.SequenceEqual(dataBytes, x.Data).Should().BeTrue());
        }
    }

    public async Task TwoClients_ExclusiveLock_SecondCannotAccess()
    {
        IFileStore fileStore1 = _getFileStore();
        IFileStore fileStore2 = _getFileStore();

        const string data = "this is a test";
        const string data2 = "this is a test";
        const string path = "acquireLease4.txt";

        var fileAccess1 = fileStore1.File(path);
        var fileAccess2 = fileStore2.File(path);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] dataBytes2 = Encoding.UTF8.GetBytes(data2);

        var writeResult = await fileAccess1.Set(dataBytes, _context);
        if (writeResult.IsConflict())
        {
            var breakResult = await fileAccess1.BreakLease(_context);
            breakResult.IsOk().Should().BeTrue();
        }

        var lease1 = (await fileAccess1.Acquire(TimeSpan.FromSeconds(-1), _context)).Assert(x => x.IsOk(), x => x.ToString()).Return();

        await using (lease1)
        {
            (await fileAccess2.Set(dataBytes2, _context)).Assert(x => x.IsError(), _ => "Should fail");

            (await lease1.Get(_context))
                .Assert(x => x.IsOk(), x => x.ToString())
                .Return().Action(x => Enumerable.SequenceEqual(dataBytes, x.Data).Should().BeTrue());

            await Task.Delay(TimeSpan.FromSeconds(1));

            (await fileAccess2.Set(dataBytes2, _context)).Assert(x => x.IsError(), _ => "Should fail");

            (await fileAccess1.Get(_context))
                .Assert(x => x.IsOk(), x => x.ToString())
                .Return().Action(x => Enumerable.SequenceEqual(dataBytes, x.Data).Should().BeTrue());
        }

        (await fileAccess2.Set(dataBytes2, _context)).Assert(x => x.IsOk(), x => x.ToString());

        (await fileAccess1.Get(_context))
            .Assert(x => x.IsOk(), x => x.ToString())
            .Return().Action(x => Enumerable.SequenceEqual(dataBytes2, x.Data).Should().BeTrue());
    }
}
