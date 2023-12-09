using SpinCluster.abstraction;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class SetupOption
{
    public List<ConfigModel> Configs { get; init; } = new List<ConfigModel>();
    public List<SubscriptionModel> Subscriptions { get; init; } = new List<SubscriptionModel>();
    public List<TenantModel> Tenants { get; init; } = new List<TenantModel>();
    public List<UserCreateModel> Users { get; init; } = new List<UserCreateModel>();
    public List<AgentModel> Agents { get; init; } = new List<AgentModel>();

    public static IValidator<SetupOption> Validator { get; } = new Validator<SetupOption>()
        .RuleForEach(x => x.Configs).Validate(ConfigModel.Validator)
        .RuleForEach(x => x.Subscriptions).Validate(SubscriptionModel.Validator)
        .RuleForEach(x => x.Tenants).Validate(TenantModel.Validator)
        .RuleForEach(x => x.Users).Validate(UserCreateModel.Validator)
        .RuleForEach(x => x.Agents).Validate(AgentModel.Validator)
        .Build();
}


public static class SetupOptionTool
{
    public static Option Validate(this SetupOption subject) => SetupOption.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SetupOption subject, out Option status)
    {
        status = subject.Validate();
        return status.IsOk();
    }
}