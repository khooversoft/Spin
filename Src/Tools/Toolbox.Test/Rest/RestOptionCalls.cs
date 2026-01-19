using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Rest;
using Toolbox.TestApi;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Rest;

public class RestOptionCalls : IClassFixture<TestApiHost>
{
    private readonly TestApiHost _testApiHost;
    private readonly ILogger _logger = NullLogger.Instance;
    public RestOptionCalls(TestApiHost testApiHost) => _testApiHost = testApiHost;

    [Fact]
    public async Task OptionOk()
    {
        var client = _testApiHost.CreateClient();

        Option<Option> response = await new RestClient(client)
            .SetPath("/option")
            .GetAsync(_logger)
            .GetContent<Option>();

        response.IsOk().BeTrue();
        response.Error.BeNull();
        response.Return().StatusCode.Be(StatusCode.OK);
        response.Return().Error.BeNull();
    }

    [Fact]
    public async Task OptionWithError()
    {
        var client = _testApiHost.CreateClient();

        Option<Option> response = await new RestClient(client)
            .SetPath("/optionWithError")
            .GetAsync(_logger)
            .GetContent<Option>();

        response.IsOk().BeTrue();
        response.Error.BeNull();
        response.HasValue.BeTrue();
        response.Return().StatusCode.Be(StatusCode.BadRequest);
        response.Return().Error.Be(ModelDefaults.BadRequestResponse);
    }
}
