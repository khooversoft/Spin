using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Extensions;

public class DictionaryDeepEqualCompareTests
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

    public record RecA(string name, int age) : IComparable
    {
        public int CompareTo(object? obj)
        {
            if (obj is not RecA subject) return 1;
            if (this.Equals(subject)) return 0;
            return 1;
        }
    }

    [Fact]
    public void RecordCompare()
    {
        Dictionary<int, RecA> list1 = new()
        {
            [10] = new RecA("n1", 10),
        };
        Dictionary<int, RecA> list2 = new()
        {
            [10] = new RecA("n1", 10),
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

    // New tests

    [Fact]
    public void NullBoth_ShouldBeTrue()
    {
        DictionaryExtensions.DeepEquals<string, string>(null, null).BeTrue();
    }

    [Fact]
    public void NullLeftOrRight_ShouldBeFalse()
    {
        var right = new Dictionary<string, string>();
        DictionaryExtensions.DeepEquals<string, string>(null, right).BeFalse();
        DictionaryExtensions.DeepEquals<string, string>(right, null).BeFalse();
    }

    [Fact]
    public void SameInstance_ShouldBeTrue()
    {
        var list = new Dictionary<string, string>
        {
            ["a"] = "1"
        };

        list.DeepEquals(list).BeTrue();
    }

    [Fact]
    public void CaseInsensitive_InternalDictionaryComparer_ShouldBeTrue()
    {
        var d1 = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Key"] = 1
        };
        var d2 = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["key"] = 1
        };

        d1.DeepEquals(d2).BeTrue();
    }

    [Fact]
    public void CaseDifference_DefaultComparer_NoKeyComparer_ShouldBeFalse()
    {
        var d1 = new Dictionary<string, int>
        {
            ["Key"] = 1
        };
        var d2 = new Dictionary<string, int>
        {
            ["key"] = 1
        };

        d1.DeepEquals(d2).BeFalse();
    }

    [Fact]
    public void CaseDifference_WithCustomKeyComparer_ShouldBeTrue()
    {
        var d1 = new Dictionary<string, int>
        {
            ["Key"] = 1
        };
        var d2 = new Dictionary<string, int>
        {
            ["key"] = 1
        };

        d1.DeepEquals(d2, StringComparer.OrdinalIgnoreCase).BeTrue();
    }

    [Fact]
    public void DuplicateKeyPairs_AsSequences_ShouldBeEqual()
    {
        var l1 = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
            new("a", 1),
            new("b", 2),
        };

        var l2 = new List<KeyValuePair<string, int>>
        {
            new("b", 2),
            new("a", 1),
            new("a", 1),
        };

        l1.DeepEquals(l2).BeTrue();
    }

    [Fact]
    public void DuplicateKeyPairs_AsSequences_CountMismatch_ShouldBeFalse()
    {
        var l1 = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
            new("a", 1),
            new("b", 2),
        };

        var l2 = new List<KeyValuePair<string, int>>
        {
            new("b", 2),
            new("a", 1),
        };

        l1.DeepEquals(l2).BeFalse();
    }

    [Fact]
    public void NonDictionary_OrderIndependence_ShouldBeTrue()
    {
        var l1 = new List<KeyValuePair<string, int>>
        {
            new("x", 9),
            new("y", 10),
            new("z", 11),
        };

        var l2 = new List<KeyValuePair<string, int>>
        {
            new("z", 11),
            new("x", 9),
            new("y", 10),
        };

        l1.DeepEquals(l2).BeTrue();
    }

    [Fact]
    public void NonDictionary_CustomKeyComparer_CaseInsensitive_ShouldBeTrue()
    {
        var l1 = new List<KeyValuePair<string, int>>
        {
            new("A", 1),
        };

        var l2 = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
        };

        l1.DeepEquals(l2, StringComparer.OrdinalIgnoreCase).BeTrue();
    }

    [Fact]
    public void NullValues_ShouldHandleCorrectly()
    {
        var d1 = new Dictionary<string, string?>
        {
            ["k1"] = null,
        };

        var d2 = new Dictionary<string, string?>
        {
            ["k1"] = null,
        };

        d1.DeepEquals(d2).BeTrue();

        var d3 = new Dictionary<string, string?>
        {
            ["k1"] = "value",
        };

        d1.DeepEquals(d3).BeFalse();
    }

    [Fact]
    public void MixedDictionaryAndEnumerable_ShouldBeTrue()
    {
        var d = new Dictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2,
        };

        var l = d.ToList();

        d.DeepEquals(l).BeTrue();
        l.DeepEquals(d).BeTrue();
    }
}
