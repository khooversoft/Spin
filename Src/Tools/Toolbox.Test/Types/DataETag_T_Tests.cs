using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class DataETag_T_Tests
{
    [Fact]
    public void EmptyDataETag()
    {
        Action a = () => new DataETag<TestData>(null!);
        a.Should().Throw<ArgumentNullException>();

        Action b = () => new DataETag<TestData>(null!);
        a.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DataETagWithData()
    {
        var td = new TestData("name1", 10);
        var e = new DataETag<TestData>(td);
        e.Should().NotBeNull();
        e.Value.Should().Be(td);
        e.ETag.Should().BeNullOrEmpty();
    }


    [Fact]
    public void DataETagWithDataAndETag()
    {
        var td = new TestData("name1", 10);
        string eTag = td.ToJson().ToHashHex();
        var e = new DataETag<TestData>(td, eTag);
        e.Should().NotBeNull();
        e.Value.Should().Be(td);
        e.ETag.Should().Be(eTag);
    }

    [Fact]
    public void DataETagSerialization()
    {
        var td = new TestData("name1", 10);
        string eTag = td.ToJson().ToHashHex();
        var e = new DataETag<TestData>(td, eTag);

        string json = e.ToJson();
        DataETag<TestData> result = json.ToObject<DataETag<TestData>>().NotNull();
        result.Should().NotBeNull();
        result.Value.Should().Be(e.Value);
        result.ETag.Should().Be(e.ETag);
    }

    private record TestData(string Name, int Age);
}
