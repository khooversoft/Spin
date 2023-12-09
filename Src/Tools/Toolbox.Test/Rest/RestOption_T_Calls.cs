using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Rest;
using Toolbox.TestApi;
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

        response.StatusCode.Should().Be(StatusCode.OK);
        response.IsOk().Should().BeTrue();
        response.Error.Should().BeNull();
        response.HasValue.Should().BeTrue();
        (response.Return() == ModelDefaults.TestModel).Should().BeTrue();
    }

    [Fact]
    public async Task NormalModelCallWithError()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/testModelWithError")
            .GetAsync(_context)
            .GetContent<TestModel>();

        response.StatusCode.Should().Be(StatusCode.BadRequest);
        response.IsOk().Should().BeFalse();
        response.Error.Should().Be(ModelDefaults.BadRequestResponse);
    }

    [Fact]
    public async Task NormalModelCallBadRequestWithNoError()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/testModelBadRequestNoErrorMessage")
            .GetAsync(_context)
            .GetContent<TestModel>();

        response.StatusCode.Should().Be(StatusCode.BadRequest);
        response.IsOk().Should().BeFalse();
        response.Error.Should().BeNull();
    }

    [Fact]
    public async Task Option_T()
    {
        var client = _testApiHost.CreateClient();

        Option<Option<TestModel>> response = await new RestClient(client)
            .SetPath("/option_t")
            .GetAsync(_context)
            .GetContent<Option<TestModel>>();

        response.StatusCode.Should().Be(StatusCode.OK);
        response.IsOk().Should().BeTrue();
        response.Error.Should().BeNull();
        response.HasValue.Should().BeTrue();

        Option<TestModel> result = response.Return();
        (result == ModelDefaults.TestModel).Should().BeTrue();
    }

    [Fact]
    public async Task Option_T_Unwraped()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/option_t")
            .GetAsync(_context)
            .GetContent<Option<TestModel>>()
            .UnwrapAsync();

        response.StatusCode.Should().Be(StatusCode.OK);
        response.IsOk().Should().BeTrue();
        response.Error.Should().BeNull();
        response.HasValue.Should().BeTrue();
        (response.Return() == ModelDefaults.TestModel).Should().BeTrue();
    }

    [Fact]
    public async Task Option_T_UnwrapedWithError()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/option_t_withError")
            .GetAsync(_context)
            .GetContent<Option<TestModel>>()
            .UnwrapAsync();

        response.StatusCode.Should().Be(StatusCode.BadRequest);
        response.IsOk().Should().BeFalse();
        response.Error.Should().Be(ModelDefaults.BadRequestResponse);
        response.HasValue.Should().BeFalse();
        response.Value.Should().Be(default);
    }

    [Fact]
    public async Task Option_T_WithInternalErrorUnwrapedWithError()
    {
        var client = _testApiHost.CreateClient();

        Option<TestModel> response = await new RestClient(client)
            .SetPath("/option_t_withInternalError")
            .GetAsync(_context)
            .GetContent<Option<TestModel>>()
            .UnwrapAsync();

        response.StatusCode.Should().Be(StatusCode.BadRequest);
        response.IsOk().Should().BeFalse();
        response.Error.Should().Be(ModelDefaults.BadRequestResponse);
        response.HasValue.Should().BeFalse();
        response.Value.Should().Be(default);
    }
}
