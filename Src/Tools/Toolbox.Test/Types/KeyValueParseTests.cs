using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class KeyValueParseTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("a=v;b=v", false)]
    [InlineData("a=v/b=v", false)]
    [InlineData("a=v-b=v", false)]
    [InlineData("*", false)]
    [InlineData("**/*", false)]
    [InlineData("a", false)]
    [InlineData("a=v", true)]
    [InlineData("a,b", false)]
    [InlineData("a;b", false)]
    [InlineData("a=v,b", false)]
    [InlineData("a,b=v", false)]
    [InlineData("a=v,b=v", true)]
    [InlineData("a,b,c", false)]
    [InlineData("a=v,b,c", false)]
    [InlineData("a=v,b=v,c", false)]
    [InlineData("a=v,b=v,c=v", true)]
    [InlineData("a,b=v,c=v", false)]
    [InlineData("a,b,c=v", false)]
    [InlineData("  a=v   ,b  =  v  ,c =   v   ", true)]
    [InlineData("key=node1, t1, t2=v2", false)]
    [InlineData("key=node1, t1=v1, t2=v2", true)]
    [InlineData("key=node1, t2=v2, i:logonProvider={LoginProvider}/{ProviderKey}", true)]
    [InlineData("key=node1, t2=v2, unique:logonProvider={LoginProvider}/{ProviderKey}", true)]
    [InlineData("key=node1, t2=v2, unique:logonProvider=microsoft/user001-microsoft-id", true)]
    [InlineData("key=node1, t2=v2, unique:logonProvider='microsoft user001-microsoft-id'", true)]
    [InlineData("key next=node1", false)]
    [InlineData("journal1=/journal1/data/*.json", true)]
    public void IsSetValid(string? key, bool expected)
    {
        var result = PropertyStringSchema.KeyValuePair.Parse(key);
        result.IsOk().Should().Be(expected);
    }

    [Fact]
    public void KeyValue()
    {
        var resultOption = PropertyStringSchema.KeyValuePair.Parse("key1=value1");
        resultOption.IsOk().Should().BeTrue();

        var result = resultOption.Return();
        result.Count.Should().Be(1);
        result[0].Key.Should().Be("key1");
        result[0].Value.Should().Be("value1");
    }

    [Fact]
    public void JournalConnection()
    {
        var resultOption = PropertyStringSchema.KeyValuePair.Parse("journal1=/journal1/data");
        resultOption.IsOk().Should().BeTrue();

        var result = resultOption.Return();
        result.Count.Should().Be(1);
        result[0].Key.Should().Be("journal1");
        result[0].Value.Should().Be("/journal1/data");
    }

    [Fact]
    public void KeysAndValues()
    {
        var resultOption = PropertyStringSchema.KeyValuePair.Parse("key=node1, t2=v2, unique:logonProvider={LoginProvider}/{ProviderKey}");
        resultOption.IsOk().Should().BeTrue(resultOption.ToString());

        var result = resultOption.Return();
        result.Count.Should().Be(3);
        var cursor = result.ToCursor();

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Key.Should().Be("key");
        cursor.Current.Value.Should().Be("node1");

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Key.Should().Be("t2");
        cursor.Current.Value.Should().Be("v2");

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Key.Should().Be("unique:logonProvider");
        cursor.Current.Value.Should().Be("{LoginProvider}/{ProviderKey}");
    }

}
