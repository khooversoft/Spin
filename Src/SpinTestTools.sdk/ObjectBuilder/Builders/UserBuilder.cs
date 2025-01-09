using Microsoft.Extensions.DependencyInjection;
using SpinClient.sdk;
using Toolbox.Logging;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class UserBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<UserClient>(() => service.GetRequiredService<UserClient>());

        var test = new OptionTest();

        foreach (var user in option.Users)
        {
            Option setOption = await client.Value.Create(user, context);

            setOption.LogStatus(context, "Creating User userId={userId}", [user.UserId]);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<UserClient>(() => service.GetRequiredService<UserClient>());

        foreach (var user in option.Users)
        {
            await client.Value.Delete(user.UserId, context);
            context.LogInformation("User deleted: {user}", user.UserId);
        }

        return StatusCode.OK;
    }
}
