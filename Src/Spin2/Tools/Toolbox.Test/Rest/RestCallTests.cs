using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Rest;
using Toolbox.TestApi;
using Toolbox.Types;

namespace Toolbox.Test.Rest;

public class RestCallTests : IClassFixture<TestApiHost>
{
    private readonly TestApiHost _testApiHost;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    public RestCallTests(TestApiHost testApiHost) => _testApiHost = testApiHost;

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
    public async Task OptionOkUnwrap()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/option")
            .GetAsync(_context)
            .GetContent<Option>()
            .UnwrapAsync();

        response.StatusCode.Should().Be(StatusCode.OK);
        response.Error.Should().BeNull();
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

    [Fact]
    public async Task OptionWithErrorUnwrap()
    {
        var client = _testApiHost.CreateClient();

        Option response = await new RestClient(client)
            .SetPath("/optionWithError")
            .GetAsync(_context)
            .GetContent<Option>()
            .UnwrapAsync();

        response.StatusCode.Should().Be(StatusCode.BadRequest);
        response.Error.Should().Be(ModelDefaults.BadRequestResponse);
    }
    
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
}
