using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class SbAccountBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        SoftBankClient softBankClient = service.GetRequiredService<SoftBankClient>();

        var test = new OptionTest();

        foreach (var account in option.SbAccounts)
        {
            var createOption = await softBankClient.Create(account, context);

            context.Trace().LogStatus(createOption, "Creating Account accountId={accountId}", account.AccountId);
            test.Test(() => createOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        SoftBankClient client = service.GetRequiredService<SoftBankClient>();

        foreach (var item in option.SbAccounts)
        {
            await client.Delete(item.AccountId, context);
            context.Trace().LogInformation("Account deleted: {accountId}", item.AccountId);
        }

        return StatusCode.OK;
    }
}
