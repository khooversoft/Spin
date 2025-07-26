//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.Types;

//public class DataETag_T_Tests
//{
//    [Fact]
//    public void EmptyDataETag()
//    {
//        Verify.Throw<ArgumentNullException>(() =>
//        {
//            new DataETag<TestData>(null!);
//        });

//        Verify.Throw<ArgumentNullException>(() =>
//        {
//            new DataETag<TestData>(null!, null);
//        });
//    }

//    [Fact]
//    public void DataETagWithData()
//    {
//        var td = new TestData("name1", 10);
//        var e = new DataETag<TestData>(td);
//        e.NotNull();
//        e.Value.Assert(x => x == td);
//        e.ETag.BeEmpty();
//    }

//    [Fact]
//    public void DataETagWithDataAndETag()
//    {
//        var td = new TestData("name1", 10);
//        string eTag = td.ToJson().ToHashHex();
//        var e = new DataETag<TestData>(td, eTag);
//        e.NotNull();
//        e.Value.Assert(x => x == td);
//        e.ETag.Be(eTag);
//    }

//    [Fact]
//    public void DataETagSerialization()
//    {
//        var td = new TestData("name1", 10);
//        string eTag = td.ToJson().ToHashHex();
//        var e = new DataETag<TestData>(td, eTag);

//        string json = e.ToJson();
//        DataETag<TestData> result = json.ToObject<DataETag<TestData>>().NotNull();
//        result.NotNull();
//        result.Value.Assert(x => x == e.Value);
//        result.ETag.Be(e.ETag);
//    }

//    private record TestData(string Name, int Age);
//}
