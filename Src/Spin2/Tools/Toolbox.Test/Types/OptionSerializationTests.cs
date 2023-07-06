using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionSerializationTests
{
    [Fact]
    public void OptionSerialzationJustStatusTest()
    {
        var option = new Option(StatusCode.OK);

        var json = Json.Default.Serialize(option);
        json.Should().NotBeNullOrEmpty();

        Option readOption = Json.Default.Deserialize<Option>(json);
        readOption.StatusCode.Should().Be(option.StatusCode);
        readOption.Error.Should().Be(option.Error);
    }

    [Fact]
    public void OptionSerialzationWithErrorTest()
    {
        var option = new Option(StatusCode.NotFound, "Error message");

        var json = Json.Default.Serialize(option);
        json.Should().NotBeNullOrEmpty();

        Option readOption = Json.Default.Deserialize<Option>(json);
        readOption.StatusCode.Should().Be(option.StatusCode);
        readOption.Error.Should().Be(option.Error);
    }

    [Fact]
    public void OptionTSerialzationJustStatusTest()
    {
        var option = new Option<string>(StatusCode.OK);

        var json = Json.Default.Serialize(option);
        json.Should().NotBeNullOrEmpty();

        Option<string> readOption = Json.Default.Deserialize<Option<string>>(json);
        readOption.StatusCode.Should().Be(option.StatusCode);
        readOption.Value.Should().Be(option.Value);
        readOption.Error.Should().Be(option.Error);
    }

    [Fact]
    public void OptionTSerialzationWithErrorTest()
    {
        var option = new Option<string>(StatusCode.NotFound, "Error message");

        var json = Json.Default.Serialize(option);
        json.Should().NotBeNullOrEmpty();

        Option<string> readOption = Json.Default.Deserialize<Option<string>>(json);
        readOption.StatusCode.Should().Be(option.StatusCode);
        readOption.Value.Should().Be(option.Value);
        readOption.Error.Should().Be(option.Error);
    }

    [Fact]
    public void OptionTSerialzationWithValueTest()
    {
        var option = new Option<string>("value");

        var json = Json.Default.Serialize(option);
        json.Should().NotBeNullOrEmpty();

        Option<string> readOption = Json.Default.Deserialize<Option<string>>(json);
        readOption.StatusCode.Should().Be(option.StatusCode);
        readOption.Value.Should().Be(option.Value);
        readOption.Error.Should().Be(option.Error);
    }

    [Fact]
    public void OptionTSerialzationWithValueAndStatusTest()
    {
        var option = new Option<string>("value", StatusCode.NotFound);

        var json = Json.Default.Serialize(option);
        json.Should().NotBeNullOrEmpty();

        Option<string> readOption = Json.Default.Deserialize<Option<string>>(json);
        readOption.StatusCode.Should().Be(option.StatusCode);
        readOption.Value.Should().Be(option.Value);
        readOption.Error.Should().Be(option.Error);
    }
}
