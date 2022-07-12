using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Pattern;
using Xunit;

namespace Toolbox.Test.Pattern
{
    public class PathPatternTests
    {
        [Fact]
        public void BasicPath_ShouldPass()
        {
            const string name = "main";
            const string source = "root/path";

            new PatternCollection()
                .AddPattern(name, source)
                .TryMatch(source, out PatternResult? result)
                .Should().BeTrue();

            result.Should().NotBeNull();
            result!.Name.Should().Be(name);
            result.Source.Should().Be(source);
            result.Pattern.Should().Be(source);
            result.Values.Should().NotBeNull();
            result.Values.Count.Should().Be(0);
        }

        [Fact]
        public void BasicPathNotMatch_ShouldNotPass()
        {
            const string name = "main";
            const string source = "root";
            const string pattern = "root/{path}";

            new PatternCollection()
                .AddPattern(name, pattern)
                .TryMatch(source, out PatternResult? result)
                .Should().BeFalse();
        }

        [Fact]
        public void ApiPathDoesNotMatch_ShouldNotPass()
        {
            const string name = "main";
            const string source = "root";
            const string pattern = "root/{path}";

            new PatternCollection()
                .AddPattern(name, pattern)
                .TryMatch(source, out PatternResult? result)
                .Should().BeFalse();
        }

        [Fact]
        public void LikeApi_ShouldPass()
        {
            const string name = "main";
            const string source = "root/pathValue";
            const string pattern = "root/{path}";

            new PatternCollection()
                .AddPattern(name, pattern)
                .TryMatch(source, out PatternResult? result)
                .Should().BeTrue();

            result.Should().NotBeNull();
            result!.Name.Should().Be(name);
            result.Source.Should().Be(source);
            result.Pattern.Should().Be(pattern);
            result.Values.Should().NotBeNull();
            result.Values.Count.Should().Be(1);
            result.Values.TryGetValue("path", out string? value).Should().BeTrue();
            value.Should().Be("pathValue");
        }

        [Fact]
        public void LikeApiWithParameter_ShouldPass()
        {
            const string name = "main";
            const string source = "root/pathValue/5";
            const string pattern = "root/{path}/{id}";

            new PatternCollection()
                .AddPattern(name, pattern)
                .TryMatch(source, out PatternResult? result)
                .Should().BeTrue();

            result.Should().NotBeNull();
            result!.Name.Should().Be(name);
            result.Source.Should().Be(source);
            result.Pattern.Should().Be(pattern);
            result.Values.Should().NotBeNull();
            result.Values.Count.Should().Be(2);

            result.Values.TryGetValue("path", out string? pathValue).Should().BeTrue();
            pathValue.Should().Be("pathValue");

            result.Values.TryGetValue("id", out string? idValue).Should().BeTrue();
            idValue.Should().Be("5");
        }


        [Fact]
        public void TwoPatterns_ShouldPass()
        {
            const string source1 = "root/pathValue/5";
            const string source2 = "root/index";
            const string pattern1 = "root/{path}/{id}";
            const string pattern2 = "root/index";

            var collection = new PatternCollection()
                .AddPattern("main1", pattern1)
                .AddPattern("main2", pattern2);

            collection
                .TryMatch(source1, out PatternResult? result)
                .Should().BeTrue();

            result.Should().NotBeNull();
            result!.Name.Should().Be("main1");
            result.Source.Should().Be(source1);
            result.Pattern.Should().Be(pattern1);
            result.Values.Should().NotBeNull();
            result.Values.Count.Should().Be(2);

            result.Values.TryGetValue("path", out string? pathValue).Should().BeTrue();
            pathValue.Should().Be("pathValue");

            result.Values.TryGetValue("id", out string? idValue).Should().BeTrue();
            idValue.Should().Be("5");

            collection
                .TryMatch(source2, out PatternResult? result2)
                .Should().BeTrue();

            result2.Should().NotBeNull();
            result2!.Name.Should().Be("main2");
            result2.Source.Should().Be(source2);
            result2.Pattern.Should().Be(pattern2);
            result2.Values.Should().NotBeNull();
            result2.Values.Count.Should().Be(0);
        }

        [Fact]
        public void ResolvePath()
        {
            PatternResult TransformFile(PatternContext context, PatternResult result)
            {
                string server = result.Values["namespace"] switch
                {
                    "contract" => "contract.server",
                    "identity" => "identity.server",

                    _ => throw new ArgumentException("unknown")
                };

                string filesystem = result.Values["environment"] switch
                {
                    "dev" =>  "testing",
                    "ppe" => "preProd",
                    "prod" => "prod",

                    _ => throw new ArgumentException("unknown")
                };

                string root = result.Values["root"];
                string file = result.Values["file"];

                return result with { Transform = $"adls://{server}/{filesystem}/{root}/{file}" };
            }

            PatternResult TransformKv(PatternContext context, PatternResult result)
            {
                string server = result.Values["namespace"] switch
                {
                    "contract" => "kv1.server",
                    "identity" => "kv2.server",

                    _ => throw new ArgumentException("unknown")
                };

                string kv = server + result.Values["environment"] switch
                {
                    "dev" => "-test",
                    "ppe" => "-ppe",
                    "prod" => "-prod",

                    _ => throw new ArgumentException("unknown")
                };

                string keyName = result.Values["keyName"];

                return result with { Transform = $"keyVault://{kv}/{keyName}" };
            }

            var collection = new PatternCollection()
                .AddPattern("file", "file://{namespace}/{environment}/{root}/{file}") // adls://server/filesystem/root/file
                .AddPattern("keyVault", "kv://{namespace}/{environment}/{keyName}") // kv://
                .AddTransform("file", TransformFile)
                .AddTransform("keyVault", TransformKv);

            collection.TryMatch("file://contract/dev/config/contract.json", out PatternResult? result1).Should().BeTrue();
            result1.Should().NotBeNull();
            result1!.Transform.Should().Be("adls://contract.server/testing/config/contract.json");

            collection.TryMatch("kv://identity/prod/keyName1", out PatternResult? result2).Should().BeTrue();
            result2.Should().NotBeNull();
            result2!.Transform.Should().Be("keyVault://kv2.server-prod/keyName1");
        }
    }
}
