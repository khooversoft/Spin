using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Extensions;

public class EnumerableExtensionTests
{
    [Fact]
    public void TwoEmptyArrays()
    {
        string[] v1 = [];
        string[] v2 = [];

        v1.IsEquivalent(v2).Should().BeTrue();
        v1.IsNotEquivalent(v2).Should().BeFalse();
    }

    [Fact]
    public void UnbalanceArrays1()
    {
        string[] v1 = [];
        string[] v2 = ["a"];

        v1.IsEquivalent(v2).Should().BeFalse();
        v1.IsNotEquivalent(v2).Should().BeTrue();
    }

    [Fact]
    public void UnbalanceArrays2()
    {
        string[] v1 = ["a"];
        string[] v2 = [];

        v1.IsEquivalent(v2).Should().BeFalse();
        v1.IsNotEquivalent(v2).Should().BeTrue();
    }

    [Fact]
    public void SingleArrays()
    {
        string[] v1 = ["a"];
        string[] v2 = ["a"];

        v1.IsEquivalent(v2).Should().BeTrue();
        v1.IsNotEquivalent(v2).Should().BeFalse();

        string[] v3 = ["b"];

        v1.IsEquivalent(v3).Should().BeFalse();
        v1.IsNotEquivalent(v3).Should().BeTrue();
    }

    [Fact]
    public void IntArrays()
    {
        int[] v1 = [10, 20, 30];
        int[] v2 = [10, 20, 30];

        v1.IsEquivalent(v2).Should().BeTrue();
        v1.IsNotEquivalent(v2).Should().BeFalse();

        int[] v3 = [10, 21, 30];

        v1.IsEquivalent(v3).Should().BeFalse();
        v1.IsNotEquivalent(v3).Should().BeTrue();
    }

    public record Rec(string name, int age);

    [Fact]
    public void RecordArrays()
    {
        Rec[] v1 = [new Rec("a", 10), new Rec("b", 20)];
        Rec[] v2 = [new Rec("a", 10), new Rec("b", 20)];

        v1.IsEquivalent(v2).Should().BeTrue();
        v1.IsNotEquivalent(v2).Should().BeFalse();

        Rec[] v3 = [new Rec("a", 10), new Rec("b", 21)];

        v1.IsEquivalent(v3).Should().BeFalse();
        v1.IsNotEquivalent(v3).Should().BeTrue();
    }

    [Fact]
    public void RecordArrayUnbalance1()
    {
        Rec[] v1 = [new Rec("a", 10), new Rec("b", 20)];
        Rec[] v2 = [new Rec("a", 10), new Rec("b", 20), new Rec("c", 30)];

        v1.IsEquivalent(v2).Should().BeFalse();
        v1.IsNotEquivalent(v2).Should().BeTrue();
    }

    [Fact]
    public void RecordArrayUnbalance2()
    {
        Rec[] v1 = [new Rec("a", 10), new Rec("b", 20), new Rec("c", 30)];
        Rec[] v2 = [new Rec("a", 10), new Rec("b", 20)];

        v1.IsEquivalent(v2).Should().BeFalse();
        v1.IsNotEquivalent(v2).Should().BeTrue();
    }
}
