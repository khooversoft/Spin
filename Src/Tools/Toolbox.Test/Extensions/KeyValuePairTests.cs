using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Extensions;

public class KeyValuePairTests
{
    [Theory]
    [InlineData("name=value", "name", "value")]
    [InlineData("name1 = value1", "name1", "value1")]
    [InlineData("name1 = value1=", "name1", "value1=")]
    public void TestToKeyValuePair(string property, string name, string value)
    {
        KeyValuePair<string, string> pair = property.ToKeyValuePair();
        pair.Key.Should().Be(name);
        pair.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("=")]
    [InlineData("name")]
    [InlineData("name=")]
    public void TestToKeyValuePair_ShouldFail(string property)
    {

        Action action = () => property.ToKeyValuePair();
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TestKeyValuePairToBind()
    {
        var lines = new[]
        {
            "Name=Name1",
            "Value=Value2",
        };

        TestRecord testRecord = lines.ToConfiguration()
            .Bind<TestRecord>()
            .NotNull(name: "Parsing error");

        testRecord.Name.Should().Be("Name1");
        testRecord.Value.Should().Be("Value2");
    }

    [Fact]
    public void TestKeyValuePairToBind_WithSpace()
    {
        var lines = new[]
        {
            "Name = Name1",
            "Value =  Value2",
        };

        TestRecord testRecord = lines.ToConfiguration()
            .Bind<TestRecord>()
            .NotNull(name: "Parsing error");

        testRecord.Name.Should().Be("Name1");
        testRecord.Value.Should().Be("Value2");
    }


    [Fact]
    public void TestKeyValuePairToBind_ShouldFail()
    {
        var lines = new[]
        {
            "Name=",
            "Value=Value2",
        };

        Action action = () => lines.ToConfiguration();

        action.Should().Throw<ArgumentException>();
    }

    private record TestRecord
    {
        public string Name { get; init; } = null!;

        public string Value { get; init; } = null!;
    }
}
