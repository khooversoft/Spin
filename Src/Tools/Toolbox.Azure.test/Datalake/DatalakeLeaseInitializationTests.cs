using Toolbox.Azure.test.Application;
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

        var deleteOption = await _dataLake.File(path).Delete(_context);
        _context.LogInformation("Delete file {path}", path);

        Option<IFileLeasedAccess> leaseOption = await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context);
        leaseOption.BeOk();

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        await using (var leaseLock = leaseOption.Return())
        {
            Option<IFileLeasedAccess> leaseOptionTemp = await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context);
            leaseOptionTemp.BeError();

            var tryAccess = await _dataLake.File(path).Set(testData, _context);
            tryAccess.IsLocked().BeTrue();

            var writeOption = await leaseOption.Return().Set(testData2, _context);
            writeOption.BeOk();

            var readOption = await _dataLake.File(path).Get(_context);
            readOption.BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        var tryAccess2 = await _dataLake.File(path).Set(testData, _context);
        tryAccess2.BeOk();

        var readOption2 = await _dataLake.File(path).Get(_context);
        readOption2.BeOk();
        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
    }


    [Fact]
    public async Task AcquireExclusiveLeaseOnNotExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        var deleteOption = await _dataLake.File(path).Delete(_context);
        _context.LogInformation("Delete file {path}", path);

        Option<IFileLeasedAccess> leaseOption = await _dataLake.File(path).AcquireExclusive(false, _context);
        leaseOption.BeOk();

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        await using (var leaseLock = leaseOption.Return())
        {
            Option<IFileLeasedAccess> leaseOptionTemp = await _dataLake.File(path).AcquireExclusive(false, _context);
            leaseOptionTemp.BeError();

            var tryAccess = await _dataLake.File(path).Set(testData, _context);
            tryAccess.IsLocked().BeTrue();

            var writeOption = await leaseOption.Return().Set(testData2, _context);
            writeOption.BeOk();

            var readOption = await _dataLake.File(path).Get(_context);
            readOption.BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        var tryAccess2 = await _dataLake.File(path).Set(testData, _context);
        tryAccess2.BeOk();

        var readOption2 = await _dataLake.File(path).Get(_context);
        readOption2.BeOk();
        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
    }

    [Fact]
    public async Task AcquireLeaseOnExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        var setupOption = await _dataLake.File(path).Set(testData, _context);
        _context.LogInformation("Set file {path}", path);

        Option<IFileLeasedAccess> leaseOption = await _dataLake.File(path).Acquire(TimeSpan.FromSeconds(60), _context);
        leaseOption.BeOk();

        await using (var leaseLock = leaseOption.Return())
        {
            var tryAccess = await _dataLake.File(path).Set(testData, _context);
            tryAccess.IsLocked().BeTrue();

            var writeOption = await leaseOption.Return().Set(testData2, _context);
            writeOption.BeOk();

            var readOption = await _dataLake.File(path).Get(_context);
            readOption.BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        var tryAccess2 = await _dataLake.File(path).Set(testData, _context);
        tryAccess2.BeOk();

        var readOption2 = await _dataLake.File(path).Get(_context);
        readOption2.BeOk();
        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
    }

    [Fact]
    public async Task AcquireExclusiveLeaseOnExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();

        var setupOption = await _dataLake.File(path).Set(testData, _context);
        _context.LogInformation("Set file {path}", path);

        Option<IFileLeasedAccess> leaseOption = await _dataLake.File(path).AcquireExclusive(false, _context);
        leaseOption.BeOk();

        await using (var leaseLock = leaseOption.Return())
        {
            var tryAccess = await _dataLake.File(path).Set(testData, _context);
            tryAccess.IsLocked().BeTrue();

            var writeOption = await leaseOption.Return().Set(testData2, _context);
            writeOption.BeOk();

            var readOption = await _dataLake.File(path).Get(_context);
            readOption.BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
        }

        var tryAccess2 = await _dataLake.File(path).Set(testData, _context);
        tryAccess2.BeOk();

        var readOption2 = await _dataLake.File(path).Get(_context);
        readOption2.BeOk();
        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
    }

    [Fact]
    public async Task AcquireExclusiveAndLeaseOnExistFile()
    {
        const string path = "acquireLeaseOnNotExistFile.txt";

        var testData = "wrong data".ToDataETag();
        var testData2 = "right data".ToDataETag();
        var testData3 = "right data #3".ToDataETag();

        var setupOption = await _dataLake.File(path).Set(testData, _context);
        if (setupOption.IsLocked()) await _dataLake.File(path).BreakLease(_context);

        _context.LogInformation("Set file {path}", path);

        Option<IFileLeasedAccess> leaseOption = await _dataLake.File(path).AcquireExclusive(false, _context);
        leaseOption.BeOk();
        Option<IFileLeasedAccess> leaseOption2 = default;

        await using (var leaseLock = leaseOption.Return())
        {
            Option<IFileLeasedAccess> leaseOptionTemp = await _dataLake.File(path).AcquireExclusive(false, _context);
            leaseOptionTemp.BeError();

            var tryAccess = await _dataLake.File(path).Set(testData, _context);
            tryAccess.IsLocked().BeTrue();

            var writeOption = await leaseOption.Return().Set(testData2, _context);
            writeOption.BeOk();

            var readOption = await _dataLake.File(path).Get(_context);
            readOption.BeOk();
            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();

            // Break lease
            leaseOption2 = await _dataLake.File(path).AcquireExclusive(true, _context);
            leaseOption2.BeOk();

            var writeOption7 = await leaseOption2.Return().Set(testData3, _context);
            writeOption7.BeOk();

            var writeOption3 = await leaseOption.Return().Set(testData3, _context);
            writeOption3.IsLocked().BeTrue();

            var readOption3 = await _dataLake.File(path).Get(_context);
            readOption3.BeOk();
            readOption3.Return().Data.SequenceEqual(testData3.Data).BeTrue();
        }

        var writeOption5 = await leaseOption.Return().Set(testData3, _context);
        writeOption5.IsLocked().BeTrue();

        var writeOption6 = await leaseOption2.Return().Set(testData2, _context);
        writeOption6.BeOk();

        var tryAccess2 = await _dataLake.File(path).Set(testData, _context);
        tryAccess2.BeError();

        (await leaseOption2.Return().Release(_context)).BeOk();

        var writeOption10 = await leaseOption.Return().Set(testData, _context);
        writeOption10.IsLocked().BeTrue().BeTrue();

        var readOption11 = await _dataLake.File(path).Get(_context);
        readOption11.BeOk();
        readOption11.Return().Data.SequenceEqual(testData2.Data).BeTrue();
    }
}
