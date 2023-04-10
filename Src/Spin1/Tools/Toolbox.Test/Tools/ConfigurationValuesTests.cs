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
using Xunit.Abstractions;

namespace Toolbox.Test.Tools
{
    public class ConfigurationValuesTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ConfigurationValuesTests(ITestOutputHelper  testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void SingleClassPropertyDump_ShouldPass()
        {
            var now = DateTime.Now.Date;

            var c = new ClassB
            {
                Name = "Name1",
                Value = 99,
                Date = now,
                Float = 99.2f,
                Switch = true
            };

            IReadOnlyList<KeyValuePair<string, string>> result = c.GetConfigurationValues();

            var list = new List<(string key, string value)>
            {
                ("name", "Name1"),
                ("value", "99"),
                ("date", now.ToString("o").Replace("00.0000000", "00")),
                ("float", "99.2"),
                ("switch", "True"),
            };

            result.OrderBy(x => x.Key)
                .Zip(list.OrderBy(x => x.key))
                .Select(x => (x: x, pass: x.First.Key == x.Second.key && x.First.Value == x.Second.value))
                .Where(x => x.pass == false)
                .Func(x => x.Count() == 0 ? null : x)
                .Should().BeNull();
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

            var testResults = result.OrderBy(x => x.Key)
                .Zip(list.OrderBy(x => x.key))
                .Select(x => (x.First, x.Second, Test: x.First.Key == x.Second.key && x.First.Value == x.Second.value));

            testResults.ForEach(x => _testOutputHelper.WriteLine($"results: First={x.First}, Second={x.Second}, Test={x.Test}"));

            testResults
                .All(x => x.Test)
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
