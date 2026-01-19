using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Rest;
using Toolbox.TestApi;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Rest;

public class RestNormalCalls : IClassFixture<TestApiHost>
{
    private readonly TestApiHost _testApiHost;
    private readonly ILogger _logger = NullLogger.Instance;
    public RestNormalCalls(TestApiHost testApiHost) => _testApiHost = testApiHost;


    [Fact]
    public async Task SimpleHelloCall()
    {
        var client = _testApiHost.CreateClient();

        var response = await client.GetStringAsync("/hello");
        response.Be("hello");
    }

    [Fact]
    public async Task SimpleHelloCallWithRestClient()
    {
        var client = _testApiHost.CreateClient();

        Option<string> response = await new RestClient(client)
            .SetPath("/hello")
            .GetAsync(_logger)
            .GetContent<string>();

        response.HasValue.BeTrue();
        response.IsOk().BeTrue();
        response.Return().Be("hello");
    }

    [Fact]
    public async Task SimpleHelloCallBadRequest()
    {
        var client = _testApiHost.CreateClient();

        Option<string> response = await new RestClient(client)
            .SetPath("/helloWithError")
            .GetAsync(_logger)
            .GetContent<string>();

        response.IsError().BeTrue();
        response.StatusCode.Be(StatusCode.BadRequest);
        response.Error.Be("badRequest for hello");
        response.HasValue.BeFalse();
    }

    [Fact]
    public async Task NormalCallWithJustStatusCodeReturned()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/statusCodeOnlyCall")
            .GetAsync(_logger)
            .ToOption();

        response.StatusCode.IsError().BeTrue();
        response.StatusCode.Be(StatusCode.Conflict);
        response.Error.BeNull();
    }

    [Fact]
    public async Task NormalCallWithTypeJustStatusCodeReturned()
    {
        var client = _testApiHost.CreateClient();

        Option<string> response = await new RestClient(client)
            .SetPath("/statusCodeOnlyCall")
            .GetAsync(_logger)
            .GetContent<string>();

        response.IsError().BeTrue();
        response.StatusCode.Be(StatusCode.Conflict);
        response.Error.BeNull();
        response.HasValue.BeFalse();
        response.Value.BeNull();
    }

    [Fact]
    public async Task JustOk()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/justOk")
            .GetAsync(_logger)
            .ToOption();

        response.StatusCode.Be(StatusCode.OK);
        response.StatusCode.IsError().BeFalse();
        response.Error.BeNull();
    }

    [Fact]
    public async Task JustOkWithModel()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/justOkWithModel")
            .GetAsync(_logger)
            .GetContent<TestModel>();

        response.StatusCode.Be(StatusCode.OK);
        response.StatusCode.IsError().BeFalse();
        response.Error.BeNull();
        (response.Return() == ModelDefaults.TestModel).BeTrue();
    }

    [Fact]
    public async Task JustOkWithMessage()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/justOkWithMessage")
            .GetAsync(_logger)
            .ToOption();

        response.StatusCode.Be(StatusCode.OK);
        response.StatusCode.IsError().BeFalse();
        response.Error.Be("this works");
    }
}
