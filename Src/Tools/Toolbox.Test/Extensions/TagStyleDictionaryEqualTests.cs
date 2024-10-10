using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Test.Extensions;

public class TagStyleDictionaryEqualTests
{
    [Fact]
    public void EmptyList()
    {
        Dictionary<string, string?> list1 = new();
        Dictionary<string, string?> list2 = new();

        list1.DeepEquals(list2).Should().BeTrue();
    }

    [Fact]
    public void OneNotEmptyList()
    {
        Dictionary<string, string?> list1 = new();
        Dictionary<string, string?> list2 = new()
        {
            ["key1"] = null,
        };

        list1.DeepEquals(list2).Should().BeFalse();

        Dictionary<string, string?> list3 = new()
        {
            ["key1"] = null,
        };
        Dictionary<string, string?> list4 = new();

        list3.DeepEquals(list4).Should().BeFalse();
    }

    [Fact]
    public void TwoSingleListWithNullValueEqual()
    {
        Dictionary<string, string?> list1 = new()
        {
            ["key1"] = null,
        };
        Dictionary<string, string?> list2 = new()
        {
            ["key1"] = null,
        };

        list1.DeepEquals(list2).Should().BeTrue();
    }

    [Fact]
    public void TwoSingleListWithNotEqualValue()
    {
        Dictionary<string, string?> list1 = new()
        {
            ["key1"] = "value1",
        };
        Dictionary<string, string?> list2 = new()
        {
            ["key1"] = null,
        };

        list1.DeepEquals(list2).Should().BeFalse();
    }

    [Fact]
    public void TwoSingleListWithWithEqualValue()
    {
        Dictionary<string, string?> list1 = new()
        {
            ["key1"] = "value1",
        };
        Dictionary<string, string?> list2 = new()
        {
            ["key1"] = "value1",
        };

        list1.DeepEquals(list2).Should().BeTrue();
    }


    [Fact]
    public void NotEqualCounts()
    {
        Dictionary<string, string?> list1 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };
        Dictionary<string, string?> list2 = new()
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
        Dictionary<string, string?> list1 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };
        Dictionary<string, string?> list2 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2-x",
        };

        list1.DeepEquals(list2).Should().BeFalse();
    }

    [Fact]
    public void EqualValues()
    {
        Dictionary<string, string?> list1 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };
        Dictionary<string, string?> list2 = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };

        list1.DeepEquals(list2).Should().BeTrue();
    }
}
