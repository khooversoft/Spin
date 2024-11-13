using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using Toolbox.Logging;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class SbAccountBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<SoftBankClient>(() => service.GetRequiredService<SoftBankClient>());

        var test = new OptionTest();

        foreach (var account in option.SbAccounts)
        {
            var createOption = await client.Value.Create(account, context);

            createOption.LogStatus(context, "Creating Account accountId={accountId}", account.AccountId);
            test.Test(() => createOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<SoftBankClient>(() => service.GetRequiredService<SoftBankClient>());

        foreach (var item in option.SbAccounts)
        {
            await client.Value.Delete(item.AccountId, context);
            context.LogInformation("Account deleted: {accountId}", item.AccountId);
        }

        return StatusCode.OK;
    }
}
