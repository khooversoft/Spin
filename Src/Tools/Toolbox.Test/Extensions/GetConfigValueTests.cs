using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Xunit;

namespace Toolbox.Test.Extensions
{
    public class GetConfigValueTests
    {
        [Fact]
        public void GivenSimpleClass_WhenGetProperties_ShouldSuccess()
        {
            var subject = new Simple
            {
                S1 = "string value 1",
                I1 = 100,
            };

            IReadOnlyList<string> configValues = subject.GetConfigValues();

            var expected = new string[]
            {
                "S1=string value 1",
                "I1=100",
            };

            configValues.Count.Should().Be(expected.Length);

            configValues.OrderBy(x => x)
                .Zip(expected.OrderBy(x => x), (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();
        }

        [Fact]
        public void GivenListWithinClass_WhenGetProperties_ShouldSuccess()
        {
            var subject = new ClassD
            {
                S1 = "string value 1",
                I1 = 100,
                Cs = Enumerable.Range(0, 3)
                    .Select(x => new ClassC
                    {
                        C_S1 = $"S1({x})",
                        C_I1 = x
                    })
                    .ToList(),
            };

            IReadOnlyList<string> configValues = subject.GetConfigValues();

            var expected = new string[]
            {
                "S1=string value 1",
                "I1=100",
                "Cs:0:C_S1=S1(0)",
                "Cs:0:C_I1=0",
                "Cs:1:C_S1=S1(1)",
                "Cs:1:C_I1=1",
                "Cs:2:C_S1=S1(2)",
                "Cs:2:C_I1=2",
            };

            configValues.Count.Should().Be(expected.Length);

            configValues.OrderBy(x => x)
                .Zip(expected.OrderBy(x => x), (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();
        }

        [Fact]
        public void GivenTwoLevelClasses_WhenGetProperties_ShouldSuccess()
        {
            var subject = new ClassA
            {
                S1 = "string value 1",
                I1 = 100,
                ClassB1 = new ClassB
                {
                    B_S1 = "classB1",
                    B_I1 = 500,
                },
            };

            IReadOnlyList<string> configValues = subject.GetConfigValues();

            var expected = new string[]
            {
                "S1=string value 1",
                "I1=100",
                "ClassB1:B_S1=classB1",
                "ClassB1:B_I1=500",
            };

            configValues.Count.Should().Be(expected.Length);

            configValues.OrderBy(x => x)
                .Zip(expected.OrderBy(x => x), (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();
        }

        [Fact]
        public void GivenTwoLevelTwoInstanceClasses_WhenGetProperties_ShouldSuccess()
        {
            IClassA subject = new ClassA
            {
                S1 = "string value 1",
                I1 = 100,
                ClassB1 = new ClassB
                {
                    B_S1 = "classB1",
                    B_I1 = 500,
                },
                ClassB2 = new ClassB
                {
                    B_S1 = "classB2",
                    B_I1 = 700,
                }
            };

            IReadOnlyList<string> configValues = subject.GetConfigValues();

            var expected = new string[]
            {
                "S1=string value 1",
                "I1=100",
                "ClassB1:B_S1=classB1",
                "ClassB1:B_I1=500",
                "ClassB2:B_S1=classB2",
                "ClassB2:B_I1=700",
            };

            configValues.Count.Should().Be(expected.Length);

            configValues.OrderBy(x => x)
                .Zip(expected.OrderBy(x => x), (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();
        }

        [Fact]
        public void GivenThreeLevelTwoInstanceClasses_WhenGetProperties_ShouldSuccess()
        {
            var subject = new ClassA
            {
                S1 = "string value 1",
                I1 = 100,
                ClassB1 = new ClassB
                {
                    B_S1 = "classB1",
                    B_I1 = 500,
                },
                ClassB2 = new ClassB
                {
                    B_S1 = "classB2",
                    B_I1 = 700,
                    ClassC = new ClassC
                    {
                        C_S1 = "classC",
                        C_I1 = 1000
                    }
                }
            };

            IReadOnlyList<string> configValues = subject.GetConfigValues();

            var expected = new string[]
            {
                "S1=string value 1",
                "I1=100",
                "ClassB1:B_S1=classB1",
                "ClassB1:B_I1=500",
                "ClassB2:B_S1=classB2",
                "ClassB2:B_I1=700",
                "ClassB2:ClassC:C_S1=classC",
                "ClassB2:ClassC:C_I1=1000",
            };

            configValues.Count.Should().Be(expected.Length);

            configValues.OrderBy(x => x)
                .Zip(expected.OrderBy(x => x), (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();
        }

        public class Simple
        {
            public string? S1 { get; set; }

            public int I1 { get; set; }
        }

        public interface IClassA
        {
            ClassB? ClassB1 { get; set; }

            ClassB? ClassB2 { get; set; }

            int I1 { get; set; }

            string? S1 { get; set; }
        }

        public class ClassA : IClassA
        {
            public string? S1 { get; set; }

            public int I1 { get; set; }

            public ClassB? ClassB1 { get; set; }

            public ClassB? ClassB2 { get; set; }
        }

        public class ClassB
        {
            public string? B_S1 { get; set; }

            public int B_I1 { get; set; }

            public ClassC? ClassC { get; set; }
        }


        public class ClassC
        {
            public string? C_S1 { get; set; }

            public int C_I1 { get; set; }
        }

        public class ClassD
        {
            public string? S1 { get; set; }

            public int I1 { get; set; }

            public IEnumerable<ClassC>? Cs { get; set; }
        }
    }
}
