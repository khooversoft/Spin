using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class ConfigurationValuesTests
    {
        [Fact]
        public void SingleClassPropertyDump_ShouldPass()
        {
            var now = DateTime.Now;

            var c = new ClassB
            {
                Name = "Name1",
                Value = 99,
                Date = now,
                Float = 99.33f,
                Switch = true
            };

            IReadOnlyList<KeyValuePair<string, string>> result = c.GetConfigurationValues();

            var list = new List<(string key, string value)>
            {
                ("name", "Name1"),
                ("value", "99"),
                ("date", now.ToString("o")),
                ("float", "99.33"),
                ("switch", "True"),
            };

            var xx = result.OrderBy(x => x.Key)
                .Zip(list.OrderBy(x => x.key))
                .All(x => x.First.Key == x.Second.key && x.First.Value == x.Second.value)
                .Should().BeTrue();
        }

        [Fact]
        public void DeepClassPropertyDump_ShouldPass()
        {
            var now = DateTime.Now;

            var c = new ClassA
            {
                Name = "Name'22",
                Value = 55,
                ClassB = new ClassB
                {
                    Name = "Name1",
                    Value = 99,
                    Date = now,
                    Float = 99.33f,
                    Switch = true
                }
            };

            IReadOnlyList<KeyValuePair<string, string>> result = c.GetConfigurationValues();

            var list = new List<(string key, string value)>
            {
                ("name", "Name'22"),
                ("value", "55"),
                ("classB:name", "Name1"),
                ("classB:value", "99"),
                ("classB:date", now.ToString("o")),
                ("classB:float", "99.33"),
                ("classB:switch", "True"),
            };

            var xx = result.OrderBy(x => x.Key)
                .Zip(list.OrderBy(x => x.key))
                .All(x => x.First.Key == x.Second.key && x.First.Value == x.Second.value)
                .Should().BeTrue();
        }

        private class ClassA
        {
            public string Name { get; set; } = null!;

            public int Value { get; set; }

            public ClassB ClassB { get; set; } = null!;
        }

        private class ClassB
        {
            public string Name { get; set; } = null!;
            public int Value { get; set; }
            public DateTime Date { get; set; }
            public float Float { get; set; }
            public bool Switch { get; set; }
        }
    }
}
