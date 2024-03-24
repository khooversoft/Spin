using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;


namespace Toolbox.Test;

public class JsonNodeTests
{
    [Fact]
    public void ParseTest()
    {
        string json = @"{
            ""name"": ""John"",
            ""age"": 30,
            ""address"": {
                ""street"": ""123 Main St"",
                ""city"": ""Seattle""
            }
        }";

        var doc = JsonDocument.Parse(json);
        var topic = doc.NotNull().RootElement.GetProperty("name");
        var address = doc.NotNull().RootElement.GetProperty("address");
        var street = address.GetProperty("street");
    }

    [Fact]
    public void PathFind()
    {
        string json = @"{
            ""name"": ""John"",
            ""age"": 30,
            ""address"": {
                ""street"": ""123 Main St"",
                ""city"": ""Seattle""
            }
        }";

        string path = "address:street";
        using var doc = JsonDocument.Parse(json);
        var keys = path.Split(':');

        JsonElement current = doc.RootElement;
        foreach (var key in keys)
        {
            current.TryGetProperty(key, out current).Should().BeTrue();
        }

        var value = current.GetString();
        value.Should().Be("123 Main St");
    }
}