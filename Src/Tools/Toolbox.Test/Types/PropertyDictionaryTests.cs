using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit;

namespace Toolbox.Test.Types;

public class PropertyDictionaryTests
{
    [Fact]
    public void Values_ShouldPass()
    {
        var dict = new PropertyDictionary();

        dict.Set("int_value", 5);
        dict.Set("string_value", "stringValue");
        dict.Set("decimal_value", 55.75m);
        dict.Set("float_value", (float)2.65);

        dict.Set((double)10.52f);
        dict.Set(1.75m);
        dict.Set(15);

        var complex = new ComplexProperty
        {
            Name = "third",
            Value = 1005.00m
        };
        dict.Set("complex_key", complex);
        dict.Set(complex);

        dict.Count.Should().Be(9);

        dict.Get<int>("int_value").Should().Be(5);
        dict.Get<string>("string_value").Should().Be("stringValue");
        dict.Get<decimal>("decimal_value").Should().Be(55.75m);
        dict.Get<float>("float_value").Should().Be((float)2.65);

        dict.Get<double>().Should().Be(10.52f);
        dict.Get<decimal>().Should().Be(1.75m);
        dict.Get<int>().Should().Be(15);

        var complex2 = dict.Get<ComplexProperty>("complex_key");
        (complex == complex2).Should().BeTrue();

        var complex3 = dict.Get<ComplexProperty>();
        (complex == complex3).Should().BeTrue();

        dict.Clear();
        dict.Count.Should().Be(0);
    }

    class ComplexProperty
    {
        public string Name { get; set; } = null!;
        public decimal Value { get; set; }
    }
}
