using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Extensions;

public class DictionaryDeepEqualTests
{
    [Fact]
    public void EmptyList()
    {
        Dictionary<string, string> list1 = new();
        Dictionary<string, string> list2 = new();

        list1.DeepEquals(list2).BeTrue();
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

        list1.DeepEquals(list2).BeTrue();
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

        list1.DeepEquals(list2).BeFalse();
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

        list1.DeepEquals(list2).BeTrue();
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

        list1.DeepEquals(list2).BeFalse();
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

        list1.DeepEquals(list2).BeFalse();
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

        list1.DeepEquals(list2).BeTrue();
    }

    // New tests below

    [Fact]
    public void DifferentInsertionOrderShouldStillBeEqual()
    {
        // Will FAIL with original DeepEquals due to order-sensitive key enumeration.
        var d1 = new Dictionary<int, string> { [1] = "A", [2] = "B", [3] = "C" };
        var d2 = new Dictionary<int, string> { [3] = "C", [2] = "B", [1] = "A" };

        d1.DeepEquals(d2).BeTrue();
    }

    [Fact]
    public void BothNullShouldBeEqual()
    {
        IEnumerable<KeyValuePair<int, int>>? left = null;
        IEnumerable<KeyValuePair<int, int>>? right = null;

        left.DeepEquals(right).BeTrue();
    }

    [Fact]
    public void OneNullShouldNotBeEqual()
    {
        IEnumerable<KeyValuePair<int, int>>? left = null;
        IEnumerable<KeyValuePair<int, int>>? right = new Dictionary<int, int> { [1] = 10 };

        left.DeepEquals(right).BeFalse();
    }

    [Fact]
    public void ReferenceTypeValuesWithSameContentButDifferentInstances()
    {
        var a1 = new[] { 1, 2, 3 };
        var a2 = new[] { 1, 2, 3 };

        var d1 = new Dictionary<string, int[]> { ["k"] = a1 };
        var d2 = new Dictionary<string, int[]> { ["k"] = a2 };

        // Default equality for arrays is reference equality; should be false.
        d1.DeepEquals(d2).BeFalse();
    }

    [Fact]
    public void EnumerableWithDuplicatePairsShouldBeEqual()
    {
        var list1 = new List<KeyValuePair<string, string>>
        {
            new("a", "1"),
            new("a", "1"),
            new("b", "2"),
        };
        var list2 = new List<KeyValuePair<string, string>>
        {
            new("b", "2"),
            new("a", "1"),
            new("a", "1"),
        };

        // Intended to pass in improved DeepEquals that treats sequences as multiset of (key,value).
        list1.DeepEquals(list2).BeTrue();
    }

    [Fact]
    public void EnumerableWithDuplicatePairsDifferentCountsShouldNotBeEqual()
    {
        var list1 = new List<KeyValuePair<string, string>>
        {
            new("a", "1"),
            new("a", "1"),
            new("b", "2"),
        };
        var list2 = new List<KeyValuePair<string, string>>
        {
            new("a", "1"),
            new("b", "2"),
        };

        list1.DeepEquals(list2).BeFalse();
    }

    [Fact]
    public void LargeRandomizedDictionariesAreEqualAfterShuffle()
    {
        var rnd = new Random(17);
        var d1 = Enumerable.Range(0, 500)
            .ToDictionary(i => i, i => $"V{rnd.Next(0, 1000)}");

        // Create second dictionary by shuffling pairs
        var d2 = d1.OrderBy(_ => rnd.Next()).ToDictionary(k => k.Key, v => v.Value);

        d1.DeepEquals(d2).BeTrue();
    }

    [Fact]
    public void LargeRandomizedDictionariesDetectDifference()
    {
        var rnd = new Random(19);
        var d1 = Enumerable.Range(0, 500)
            .ToDictionary(i => i, i => $"V{rnd.Next(0, 1000)}");

        var d2 = d1.ToDictionary(x => x.Key, x => x.Value);
        d2[250] = "AlteredValue";

        d1.DeepEquals(d2).BeFalse();
    }
}
