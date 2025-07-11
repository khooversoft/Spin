using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Rest;
using Toolbox.TestApi;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Rest;

public class RestOption_T_Calls : IClassFixture<TestApiHost>
{
    private readonly TestApiHost _testApiHost;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public RestOption_T_Calls(TestApiHost testApiHost) => _testApiHost = testApiHost;

    [Fact]
    public async Task NormalModelCall()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/testModel")
            .GetAsync(_context)
            .GetContent<TestModel>();

        response.StatusCode.Be(StatusCode.OK);
        response.IsOk().BeTrue();
        response.Error.BeNull();
        response.HasValue.BeTrue();
        (response.Return() == ModelDefaults.TestModel).BeTrue();
    }

    [Fact]
    public async Task NormalModelCallWithError()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/testModelWithError")
            .GetAsync(_context)
            .GetContent<TestModel>();

        response.StatusCode.Be(StatusCode.BadRequest);
        response.IsOk().BeFalse();
        response.Error.Be(ModelDefaults.BadRequestResponse);
    }

    [Fact]
    public async Task NormalModelCallBadRequestWithNoError()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/testModelBadRequestNoErrorMessage")
            .GetAsync(_context)
            .GetContent<TestModel>();

        response.StatusCode.Be(StatusCode.BadRequest);
        response.IsOk().BeFalse();
        response.Error.BeNull();
    }

    [Fact]
    public async Task Option_T()
    {
        var client = _testApiHost.CreateClient();

        Option<Option<TestModel>> response = await new RestClient(client)
            .SetPath("/option_t")
            .GetAsync(_context)
            .GetContent<Option<TestModel>>();

        response.StatusCode.Be(StatusCode.OK);
        response.IsOk().BeTrue();
        response.Error.BeNull();
        response.HasValue.BeTrue();

        Option<TestModel> result = response.Return();
        (result == ModelDefaults.TestModel).BeTrue();
    }
}
