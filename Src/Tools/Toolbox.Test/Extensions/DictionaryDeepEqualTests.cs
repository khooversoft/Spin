using Toolbox.Extensions;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Extensions;

public class DictionaryDeepEqualTests
{
    [Fact]
    public void EmptyList()
    {
        Dictionary<string, string> list1 = new();
        Dictionary<string, string> list2 = new();

        list1.DeepEquals(list2).Should().BeTrue();
    }

    [Fact]
    public void TwoSingleListWithWithEqualValueAsValueType()
    {
        Dictionary<int, long> list1 = new()
        {
            [10] = 101,
        };
        Dictionary<int, long> list2 = new()
        {
            [10] = 101,
        };

        list1.DeepEquals(list2).Should().BeTrue();
    }


    [Fact]
    public void TwoSingleListWithWithNotEqualValueAsValueType()
    {
        Dictionary<int, long> list1 = new()
        {
            [10] = 101,
        };
        Dictionary<int, long> list2 = new()
        {
            [10] = 102,
        };

        list1.DeepEquals(list2).Should().BeFalse();
    }

    [Fact]
    public void TwoSingleListWithWithEqualValue()
    {
        Dictionary<string, string> list1 = new()
        {
            ["key1"] = "value1",
        };
        Dictionary<string, string> list2 = new()
        {
            ["key1"] = "value1",
        };

        list1.DeepEquals(list2).Should().BeTrue();
    }


    [Fact]
    public void NotEqualCounts()
    {
        Dictionary<string, string> list1 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };
        Dictionary<string, string> list2 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2-x",
            ["key3"] = "value2-x",
        };

        list1.DeepEquals(list2).Should().BeFalse();
    }

    [Fact]
    public void NotEqualValues()
    {
        Dictionary<string, string> list1 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };
        Dictionary<string, string> list2 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2-x",
        };

        list1.DeepEquals(list2).Should().BeFalse();
    }

    [Fact]
    public void EqualValues()
    {
        Dictionary<string, string> list1 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };
        Dictionary<string, string> list2 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };

        list1.DeepEquals(list2).Should().BeTrue();
    }
}
