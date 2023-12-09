using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class User : ICommandRoute
{
    private readonly UserClient _client;
    private readonly ILogger<User> _logger;

    public User(UserClient client, ILogger<User> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("user", "User management")
    {
        new CommandSymbol("delete", "Delete a USer").Action(command =>
        {
            var userId = command.AddArgument<string>("userId", "Id of User");
            command.SetHandler(Delete, userId);
        }),
        new CommandSymbol("get", "Get User details").Action(command =>
        {
            var userId = command.AddArgument<string>("userId", "Id of User");
            command.SetHandler(Get, userId);
        }),
        new CommandSymbol("set", "Create or update user details").Action(command =>
        {
            var jsonFile = command.AddArgument<string>("jsonFile", "Json with Tenant details");
            command.SetHandler(Set, jsonFile);
        }),
    };

    public async Task Delete(string tenantId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Deleting tenant tenantId={tenantId}", tenantId);

        Option response = await _client.Delete(tenantId, context);
        context.Trace().LogStatus(response, "Deleting Tenant");
    }

    public async Task Get(string tenantId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Get Tenant tenantId={tenantId}", tenantId);

        var response = await _client.Get(tenantId, context);
        if (response.IsError())
        {
            context.Trace().LogError("Cannot get Tenant tenantId={tenantId}", tenantId);
            return;
        }

        var result = response.Return()
            .ToDictionary()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Tenant...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.Trace().LogInformation(result);
    }

    public async Task Set(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<UserCreateModel>(jsonFile, UserCreateModel.Validator, context);
        if (readResult.IsError()) return;

        UserCreateModel model = readResult.Return();

        Option response = await _client.Create(model, context);
        context.Trace().LogStatus(response, "Creating/Updating Tenant, model={model}", model);
    }
}
