using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Rest;
using Toolbox.TestApi;
using Toolbox.Types;

namespace Toolbox.Test.Rest;

public class RestOptionCalls : IClassFixture<TestApiHost>
{
    private readonly TestApiHost _testApiHost;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    public RestOptionCalls(TestApiHost testApiHost) => _testApiHost = testApiHost;

    [Fact]
    public async Task OptionOk()
    {
        var client = _testApiHost.CreateClient();

        Option<Option> response = await new RestClient(client)
            .SetPath("/option")
            .GetAsync(_context)
            .GetContent<Option>();

        response.IsOk().Should().BeTrue();
        response.Error.Should().BeNull();
        response.Return().StatusCode.Should().Be(StatusCode.OK);
        response.Return().Error.Should().BeNull();
    }

    [Fact]
    public async Task OptionWithError()
    {
        var client = _testApiHost.CreateClient();

        Option<Option> response = await new RestClient(client)
            .SetPath("/optionWithError")
            .GetAsync(_context)
            .GetContent<Option>();

        response.IsOk().Should().BeTrue();
        response.Error.Should().BeNull();
        response.HasValue.Should().BeTrue();
        response.Return().StatusCode.Should().Be(StatusCode.BadRequest);
        response.Return().Error.Should().Be(ModelDefaults.BadRequestResponse);
    }
}
