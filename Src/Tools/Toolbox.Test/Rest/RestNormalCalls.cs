using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Rest;
using Toolbox.TestApi;
using Toolbox.Types;

namespace Toolbox.Test.Rest;

public class RestNormalCalls : IClassFixture<TestApiHost>
{
    private readonly TestApiHost _testApiHost;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    public RestNormalCalls(TestApiHost testApiHost) => _testApiHost = testApiHost;


    [Fact]
    public async Task SimpleHelloCall()
    {
        var client = _testApiHost.CreateClient();

        var response = await client.GetStringAsync("/hello");
        response.Should().Be("hello");
    }

    [Fact]
    public async Task SimpleHelloCallWithRestClient()
    {
        var client = _testApiHost.CreateClient();

        Option<string> response = await new RestClient(client)
            .SetPath("/hello")
            .GetAsync(_context)
            .GetContent<string>();

        response.HasValue.Should().BeTrue();
        response.IsOk().Should().BeTrue();
        response.Return().Should().Be("hello");
    }

    [Fact]
    public async Task SimpleHelloCallBadRequest()
    {
        var client = _testApiHost.CreateClient();

        Option<string> response = await new RestClient(client)
            .SetPath("/helloWithError")
            .GetAsync(_context)
            .GetContent<string>();

        response.IsError().Should().BeTrue();
        response.StatusCode.Should().Be(StatusCode.BadRequest);
        response.Error.Should().Be("badRequest for hello");
        response.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task NormalCallWithJustStatusCodeReturned()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/statusCodeOnlyCall")
            .GetAsync(_context)
            .ToOption();

        response.StatusCode.IsError().Should().BeTrue();
        response.StatusCode.Should().Be(StatusCode.Conflict);
        response.Error.Should().BeNull();
    }

    [Fact]
    public async Task NormalCallWithTypeJustStatusCodeReturned()
    {
        var client = _testApiHost.CreateClient();

        Option<string> response = await new RestClient(client)
            .SetPath("/statusCodeOnlyCall")
            .GetAsync(_context)
            .GetContent<string>();

        response.IsError().Should().BeTrue();
        response.StatusCode.Should().Be(StatusCode.Conflict);
        response.Error.Should().BeNull();
        response.HasValue.Should().BeFalse();
        response.Value.Should().BeNull();
    }

    [Fact]
    public async Task JustOk()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/justOk")
            .GetAsync(_context)
            .ToOption();

        response.StatusCode.Should().Be(StatusCode.OK);
        response.StatusCode.IsError().Should().BeFalse();
        response.Error.Should().BeNull();
    }

    [Fact]
    public async Task JustOkWithModel()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/justOkWithModel")
            .GetAsync(_context)
            .GetContent<TestModel>();

        response.StatusCode.Should().Be(StatusCode.OK);
        response.StatusCode.IsError().Should().BeFalse();
        response.Error.Should().BeNull();
        (response.Return() == ModelDefaults.TestModel).Should().BeTrue();
    }

    [Fact]
    public async Task JustOkWithMessage()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/justOkWithMessage")
            .GetAsync(_context)
            .ToOption();

        response.StatusCode.Should().Be(StatusCode.OK);
        response.StatusCode.IsError().Should().BeFalse();
        response.Error.Should().Be("this works");
    }
}
