using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeLeaseInitializationTests
{
    private readonly ScopeContext _context;
    private readonly IFileStore _dataLake;
    private readonly ITestOutputHelper _output;

    public DatalakeLeaseInitializationTests(ITestOutputHelper output)
    {
        _output = output;
        _context = TestApplication.CreateScopeContext<DatalakeLeaseTests>(_output);
        _dataLake = TestApplication.GetDatalake("datalakeLeaseInitializationTests");
    }

    [Fact]
    public async Task AcquireLeaseOnNotExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        await ForceDelete(path);

        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context)).BeOk();

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        await using (var leaseLock = leaseOption.Return())
        {
            (await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context)).BeError();
            (await _dataLake.File(path).AcquireExclusive(false, _context)).BeError();
            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

            (await leaseOption.Return().Set(testData2, _context)).BeOk();

            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        (await _dataLake.File(path).Set(testData, _context)).BeOk();

        var readOption2 = (await _dataLake.File(path).Get(_context)).BeOk();
        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
    }


    [Fact]
    public async Task AcquireExclusiveLeaseOnNotExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        await ForceDelete(path);

        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireExclusive(false, _context)).BeOk();

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        await using (var leaseLock = leaseOption.Return())
        {
            (await _dataLake.File(path).AcquireExclusive(false, _context)).BeError();
            (await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context)).BeError();
            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

            (await leaseOption.Return().Set(testData2, _context)).BeOk();

            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        (await _dataLake.File(path).Set(testData, _context)).BeOk();

        var readOption2 = (await _dataLake.File(path).Get(_context)).BeOk();
        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
    }

    [Fact]
    public async Task AcquireLeaseOnExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        await ForceSet(path, testData);

        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context)).BeOk();

        await using (var leaseLock = leaseOption.Return())
        {
            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();
            (await leaseOption.Return().Set(testData2, _context)).BeOk();

            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        var tryAccess2 = (await _dataLake.File(path).Set(testData, _context)).BeOk();

        var readOption2 = (await _dataLake.File(path).Get(_context)).BeOk();
        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
    }

    [Fact]
    public async Task AcquireExclusiveLeaseOnExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        await ForceSet(path, testData);

        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireExclusive(false, _context)).BeOk();

        await using (var leaseLock = leaseOption.Return())
        {
            (await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context)).BeError();
            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

            (await leaseOption.Return().Set(testData2, _context)).BeOk();

            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        (await _dataLake.File(path).Set(testData, _context)).BeOk();

        (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData.Data).BeTrue();
    }

    [Fact]
    public async Task AcquireExclusiveAndLeaseOnExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();
        var testData3 = "right data #3".ToDataETag();

        await ForceSet(path, testData);

        _context.LogInformation("Set file {path}", path);

        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireExclusive(false, _context)).BeOk();
        Option<IFileLeasedAccess> leaseOption2 = default;

        await using (var leaseLock = leaseOption.Return())
        {
            (await _dataLake.File(path).AcquireExclusive(false, _context)).BeError();
            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

            (await leaseOption.Return().Set(testData2, _context)).BeOk();
            (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData2.Data).BeTrue();

            // Break lease
            leaseOption2 = (await _dataLake.File(path).AcquireExclusive(true, _context)).BeOk();
            (await leaseOption2.Return().Set(testData3, _context)).BeOk();

            (await leaseOption.Return().Set(testData3, _context)).IsLocked().BeTrue();

            var readOption3 = (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData3.Data).BeTrue();
        }

        (await leaseOption.Return().Set(testData3, _context)).IsLocked().BeTrue();

        (await leaseOption2.Return().Set(testData2, _context)).BeOk();

        (await _dataLake.File(path).Set(testData, _context)).BeError();

        (await leaseOption2.Return().Release(_context)).BeOk();

        (await leaseOption.Return().Set(testData, _context)).IsLocked().BeTrue();

        (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData2.Data).BeTrue();
    }

    private async Task ForceDelete(string path)
    {
        var deleteOption = (await _dataLake.File(path).Delete(_context)).LogStatus(_context, "Delete file {path}", [path]);
        if (deleteOption.IsOk() || !deleteOption.IsLocked()) return;

        var breakOption = (await _dataLake.File(path).BreakLease(_context)).LogStatus(_context, "Break lease {path}", [path]);
        breakOption.BeOk();

        (await _dataLake.File(path).Delete(_context)).LogStatus(_context, "Delete file {path}", [path]).BeOk();
    }

    private async Task ForceSet(string path, DataETag data)
    {
        var writeOption = (await _dataLake.File(path).Set(data, _context)).LogStatus(_context, "Delete file {path}", [path]);
        if (writeOption.IsOk() || !writeOption.IsLocked()) return;

        var breakOption = (await _dataLake.File(path).BreakLease(_context)).LogStatus(_context, "Break lease {path}", [path]);
        breakOption.BeOk();

        (await _dataLake.File(path).Set(data, _context)).LogStatus(_context, "Delete file {path}", [path]).BeOk();
    }
}
