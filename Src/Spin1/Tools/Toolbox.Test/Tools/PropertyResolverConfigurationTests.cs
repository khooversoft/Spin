using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class PropertyResolverConfigurationTests
    {
        [Fact]
        public void NoPropertyResolver_ShouldPass()
        {
            var dict = new Dictionary<string, string?>
            {
                ["key1"] = "value1",
                ["key1:subKey1"] = "value1-1",
                ["key2"] = "value2",
                ["key2:subKey2"] = "value1-2",
            };

            var secrets = new Dictionary<string, string>();

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .AddPropertyResolver(secrets)
                .Build()
                .ResolveProperties();

            IReadOnlyList<KeyValuePair<string, string>> results = config
                .AsEnumerable()
                .Where(x => x.Value != null)
                .OfType<KeyValuePair<string, string>>()
                .ToArray();

            dict.Keys.OrderBy(x => x).SequenceEqual(results.Select(x => x.Key).OrderBy(x => x));
            dict.Values.OrderBy(x => x).SequenceEqual(results.Select(x => x.Value).OrderBy(x => x));

            config["key1"].Should().Be("value1");
            config["key1:subKey1"].Should().Be("value1-1");
        }

        [Fact]
        public void WithPropertyResolver_ShouldPass()
        {
            var dict = new Dictionary<string, string?>
            {
                ["key1"] = "value1={secretKey}",
                ["key1:subKey1"] = "value1-1",
                ["key2"] = "value2",
                ["key2:subKey2"] = "value1-2={secretValue}",
            };

            var secrets = new Dictionary<string, string>
            {
                ["secretKey"] = "MyLady",
                ["secretValue"] = "myValue",
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .AddPropertyResolver(secrets)
                .Build()
                .ResolveProperties();

            IReadOnlyList<KeyValuePair<string, string>> results = config
                .AsEnumerable()
                .Where(x => x.Value != null)
                .OfType<KeyValuePair<string, string>>()
                .ToArray();

            config["key1"].Should().Be("value1=MyLady");
            config["key2:subKey2"].Should().Be("value1-2=myValue");
        }
    }
}