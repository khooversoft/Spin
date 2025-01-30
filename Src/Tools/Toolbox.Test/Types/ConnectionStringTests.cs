using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ConnectionStringTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("a=v,b=v", false)]
    [InlineData("a=v/b=v", false)]
    [InlineData("a=v-b=v", false)]
    [InlineData("*", false)]
    [InlineData("a", true)]
    [InlineData("a=v", true)]
    [InlineData("a;b", true)]
    [InlineData("a,b", false)]
    [InlineData("a=v;b", true)]
    [InlineData("a;b=v", true)]
    [InlineData("a=v;b=v", true)]
    [InlineData("a;b;c", true)]
    [InlineData("a=v;b;c", true)]
    [InlineData("a=v;b=v;c", true)]
    [InlineData("a=v;b=v;c=v", true)]
    [InlineData("a;b=v;c=v", true)]
    [InlineData("a;b;c=v", true)]
    [InlineData("  a=v   ;b  =  v  ;c =   v   ", true)]
    [InlineData("key=node1; t1; t2=v2", true)]
    [InlineData("key=node1; t1; t2=v2; i:logonProvider={LoginProvider}/{ProviderKey}", true)]
    [InlineData("key=node1; t1; t2=v2; unique:logonProvider={LoginProvider}/{ProviderKey}", true)]
    [InlineData("key=node1; t1; t2=v2; unique:logonProvider=microsoft/user001-microsoft-id", true)]
    [InlineData("key=node1; t1; t2=v2; unique:logonProvider='microsoft user001-microsoft-id'", true)]
    [InlineData("key next=node1", false)]
    public void IsSetValid(string? key, bool expected)
    {
        var result = PropertyStringSchema.ConnectionString.Parse(key);
        result.IsOk().Should().Be(expected);
    }

    [Fact]
    public void SingleKey()
    {
        var resultOption = PropertyStringSchema.ConnectionString.Parse("key1");
        resultOption.IsOk().Should().BeTrue();

        var result = resultOption.Return();
        result.Count.Should().Be(1);
        result[0].Key.Should().Be("key1");
        result[0].Value.BeNull();
    }

    [Fact]
    public void KeyValue()
    {
        var resultOption = PropertyStringSchema.ConnectionString.Parse("key1=value1");
        resultOption.IsOk().Should().BeTrue();

        var result = resultOption.Return();
        result.Count.Should().Be(1);
        result[0].Key.Should().Be("key1");
        result[0].Value.Should().Be("value1");
    }

    [Fact]
    public void JournalConnection()
    {
        var resultOption = PropertyStringSchema.ConnectionString.Parse("journal1=/journal1/data");
        resultOption.IsOk().Should().BeTrue();

        var result = resultOption.Return();
        result.Count.Should().Be(1);
        result[0].Key.Should().Be("journal1");
        result[0].Value.Should().Be("/journal1/data");
    }

    [Fact]
    public void KeysAndValues()
    {
        var resultOption = PropertyStringSchema.ConnectionString.Parse("key=node1; t1; t2=v2; unique:logonProvider={LoginProvider}/{ProviderKey}");
        resultOption.IsOk().Should().BeTrue(resultOption.ToString());

        var result = resultOption.Return();
        result.Count.Should().Be(4);
        var cursor = result.ToCursor();

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Key.Should().Be("key");
        cursor.Current.Value.Should().Be("node1");

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Key.Should().Be("t1");
        cursor.Current.Value.BeNull();

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Key.Should().Be("t2");
        cursor.Current.Value.Should().Be("v2");

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Key.Should().Be("unique:logonProvider");
        cursor.Current.Value.Should().Be("{LoginProvider}/{ProviderKey}");
    }
}
