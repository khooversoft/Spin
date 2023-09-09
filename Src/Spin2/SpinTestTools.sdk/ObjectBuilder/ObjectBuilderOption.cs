using System.Reflection;
using Microsoft.Azure.Amqp.Framing;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder;

public record ObjectBuilderOption
{
    public List<SubscriptionModel> Subscriptions { get; init; } = new List<SubscriptionModel>();
    public List<TenantModel> Tenants { get; init; } = new List<TenantModel>();
    public List<UserCreateModel> Users { get; init; } = new List<UserCreateModel>();
    public List<AccountDetail> Accounts { get; init; } = new List<AccountDetail>();
    public List<LedgerItem> LedgerItems { get; init; } = new List<LedgerItem>();
    public List<AgentModel> Agents { get; init; } = new List<AgentModel>();
    public List<SmartcModel> SmartcItems { get; init; } = new List<SmartcModel>();

    public static IValidator<ObjectBuilderOption> Validator { get; } = new Validator<ObjectBuilderOption>()
        .RuleForEach(x => x.Subscriptions).Validate(SubscriptionModel.Validator)
        .RuleForEach(x => x.Tenants).Validate(TenantModel.Validator)
        .RuleForEach(x => x.Users).Validate(UserCreateModel.Validator)
        .RuleForEach(x => x.Accounts).Validate(AccountDetail.Validator)
        .RuleForEach(x => x.LedgerItems).Validate(LedgerItem.Validator)
        .RuleForEach(x => x.Agents).Validate(AgentModel.Validator)
        .RuleForEach(x => x.SmartcItems).Validate(SmartcModel.Validator)
        .Build();
}


public static class ObjectBuilderOptionTool
{
    public static ObjectBuilderOption ReadFromResource(Type type, string path)
    {
        using Stream resource = Assembly.GetAssembly(type)
            .NotNull()
            .GetManifestResourceStream(path)
            .NotNull($"Cannot find resource {path}")!;

        using StreamReader reader = new StreamReader(resource);
        string data = reader.ReadToEnd();

        ObjectBuilderOption option = data.ToObject<ObjectBuilderOption>().NotNull();

        var v = option.Validate().ThrowOnError();
        return option;
    }

    public static ObjectBuilderOption ReadFromJson(string json)
    {
        ObjectBuilderOption option = json.ToObject<ObjectBuilderOption>().NotNull();

        var v = option.Validate().ThrowOnError();
        return option;
    }

    public static Option Validate(this ObjectBuilderOption subject) => ObjectBuilderOption.Validator.Validate(subject).ToOptionStatus();
}