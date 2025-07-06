//using Toolbox.Azure.test.Application;
//using Toolbox.Test.Store;
//using Xunit.Abstractions;

//namespace Toolbox.Azure.test.Datalake;

//public class JournalLogBackgroundTests
//{
//    private readonly JournalLogBackgroundStandardTests _test;

//    public JournalLogBackgroundTests(ITestOutputHelper outputHelper)
//    {
//        var fileStore = TestApplication.GetDatalake("datastore-tests");
//        _test = new JournalLogBackgroundStandardTests(fileStore, outputHelper);
//    }

//    [Fact]
//    public Task AddSingleJournal()
//    {
//        return _test.AddSingleJournal();
//    }

//    [Fact]
//    public Task AddMultipleJournal()
//    {
//        return _test.AddMultipleJournal();
//    }

//    [Fact]
//    public Task AddMultipleBatchJournal()
//    {
//        return _test.AddMultipleBatchJournal();
//    }
//}
