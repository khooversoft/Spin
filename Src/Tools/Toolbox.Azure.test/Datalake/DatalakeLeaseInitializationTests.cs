//using Toolbox.Azure.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Azure.test.Datalake;

//public abstract class ParallelBaseTests
//{
//    protected readonly ScopeContext _context;
//    protected readonly IFileStore _dataLake;
//    protected readonly ITestOutputHelper _output;

//    public ParallelBaseTests(ITestOutputHelper output)
//    {
//        _output = output;
//        _context = TestApplication.CreateScopeContext<ParallelBaseTests>(_output);
//        _dataLake = TestApplication.GetDatalake("datalakeLeaseInitializationTests");
//    }
//}

//public class AcquireLeaseOnNotExistFileTest : ParallelBaseTests
//{
//    public AcquireLeaseOnNotExistFileTest(ITestOutputHelper output) : base(output) { }

//    [Fact]
//    public async Task AcquireLeaseOnNotExistFile()
//    {
//        const string path = nameof(AcquireLeaseOnNotExistFile) + ".txt";

//        (await _dataLake.File(path).ForceDelete(_context)).BeOk();

//        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireLease(TimeSpan.FromSeconds(60), _context)).BeOk();

//        var testData = "wrong data".ToDataETag();
//        var testData2 = "right data".ToDataETag();

//        await using (var leaseLock = leaseOption.Return())
//        {
//            (await _dataLake.File(path).AcquireLease(TimeSpan.FromSeconds(60), _context)).BeError();
//            (await _dataLake.File(path).AcquireExclusiveLease(false, _context)).BeError();
//            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

//            (await leaseOption.Return().Set(testData2, _context)).BeOk();

//            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
//            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
//        }

//        (await _dataLake.File(path).Set(testData, _context)).BeOk();

//        var readOption2 = (await _dataLake.File(path).Get(_context)).BeOk();
//        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
//    }
//}

//public class AcquireExclusiveLeaseOnNotExistFileTest : ParallelBaseTests
//{
//    public AcquireExclusiveLeaseOnNotExistFileTest(ITestOutputHelper output) : base(output) { }

//    [Fact]
//    public async Task AcquireExclusiveLeaseOnNotExistFile()
//    {
//        const string path = nameof(AcquireExclusiveLeaseOnNotExistFile) + ".txt";

//        (await _dataLake.File(path).ForceDelete(_context)).BeOk();

//        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireExclusiveLease(false, _context)).BeOk();

//        var testData = "wrong data".ToDataETag();
//        var testData2 = "right data".ToDataETag();

//        await using (var leaseLock = leaseOption.Return())
//        {
//            (await _dataLake.File(path).AcquireExclusiveLease(false, _context)).BeError();
//            (await _dataLake.File(path).AcquireLease(TimeSpan.FromSeconds(60), _context)).BeError();
//            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

//            (await leaseOption.Return().Set(testData2, _context)).BeOk();

//            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
//            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
//        }

//        (await _dataLake.File(path).Set(testData, _context)).BeOk();

//        var readOption2 = (await _dataLake.File(path).Get(_context)).BeOk();
//        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
//    }
//}

//public class AcquireLeaseOnExistFileTest : ParallelBaseTests
//{
//    public AcquireLeaseOnExistFileTest(ITestOutputHelper output) : base(output) { }

//    [Fact]
//    public async Task AcquireLeaseOnExistFile()
//    {
//        const string path = nameof(AcquireLeaseOnExistFile) + ".txt";

//        var testData = "wrong data".ToDataETag();
//        var testData2 = "right data".ToDataETag();

//        (await _dataLake.File(path).ForceSet(testData, _context)).BeOk();

//        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireLease(TimeSpan.FromSeconds(60), _context)).BeOk();

//        await using (var leaseLock = leaseOption.Return())
//        {
//            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();
//            (await leaseOption.Return().Set(testData2, _context)).BeOk();

//            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
//            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
//        }

//        var tryAccess2 = (await _dataLake.File(path).Set(testData, _context)).BeOk();

//        var readOption2 = (await _dataLake.File(path).Get(_context)).BeOk();
//        readOption2.Return().Data.SequenceEqual(testData.Data).BeTrue();
//    }
//}


//public class AcquireExclusiveLeaseOnExistFileTest : ParallelBaseTests
//{
//    public AcquireExclusiveLeaseOnExistFileTest(ITestOutputHelper output) : base(output) { }

//    [Fact]
//    public async Task AcquireExclusiveLeaseOnExistFile()
//    {
//        const string path = nameof(AcquireExclusiveLeaseOnExistFile) + ".txt";

//        var testData = "wrong data".ToDataETag();
//        var testData2 = "right data".ToDataETag();

//        (await _dataLake.File(path).ForceSet(testData, _context)).BeOk();

//        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireExclusiveLease(false, _context)).BeOk();

//        await using (var leaseLock = leaseOption.Return())
//        {
//            (await _dataLake.File(path).AcquireLease(TimeSpan.FromSeconds(60), _context)).BeError();
//            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

//            (await leaseOption.Return().Set(testData2, _context)).BeOk();

//            var readOption = (await _dataLake.File(path).Get(_context)).BeOk();
//            readOption.Return().Data.SequenceEqual(testData2.Data).BeTrue();
//        }

//        (await _dataLake.File(path).Set(testData, _context)).BeOk();

//        (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData.Data).BeTrue();
//    }
//}


//public class AcquireExclusiveAndLeaseOnExistFileTest : ParallelBaseTests
//{
//    public AcquireExclusiveAndLeaseOnExistFileTest(ITestOutputHelper output) : base(output) { }


//    [Fact]
//    public async Task AcquireExclusiveAndLeaseOnExistFile()
//    {
//        const string path = nameof(AcquireExclusiveAndLeaseOnExistFile) + ".txt";

//        var testData = "wrong data".ToDataETag();
//        var testData2 = "right data".ToDataETag();
//        var testData3 = "right data #3".ToDataETag();

//        (await _dataLake.File(path).ForceSet(testData, _context)).BeOk();

//        _context.LogInformation("Set file {path}", path);

//        Option<IFileLeasedAccess> leaseOption = (await _dataLake.File(path).AcquireExclusiveLease(false, _context)).BeOk();
//        Option<IFileLeasedAccess> leaseOption2 = default;

//        await using (var leaseLock = leaseOption.Return())
//        {
//            (await _dataLake.File(path).AcquireExclusiveLease(false, _context)).BeError();
//            (await _dataLake.File(path).Set(testData, _context)).IsLocked().BeTrue();

//            (await leaseOption.Return().Set(testData2, _context)).BeOk();
//            (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData2.Data).BeTrue();

//            // Break lease
//            leaseOption2 = (await _dataLake.File(path).AcquireExclusiveLease(true, _context)).BeOk();
//            (await leaseOption2.Return().Set(testData3, _context)).BeOk();

//            (await leaseOption.Return().Set(testData3, _context)).IsLocked().BeTrue();

//            var readOption3 = (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData3.Data).BeTrue();
//        }

//        (await leaseOption.Return().Set(testData3, _context)).IsLocked().BeTrue();

//        (await leaseOption2.Return().Set(testData2, _context)).BeOk();

//        (await _dataLake.File(path).Set(testData, _context)).BeError();

//        (await leaseOption2.Return().Release(_context)).BeOk();

//        (await leaseOption.Return().Set(testData, _context)).IsLocked().BeTrue();

//        (await _dataLake.File(path).Get(_context)).BeOk().Return().Data.SequenceEqual(testData2.Data).BeTrue();
//    }
//}
