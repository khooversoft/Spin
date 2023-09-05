using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SoftBank.sdk.Models;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinClusterCmd.Application;

internal record ScenarioOption
{
    public IReadOnlyList<SubscriptionOption> Subscriptions { get; init; } = Array.Empty<SubscriptionOption>();
    public IReadOnlyList<TenantOption> Tenants { get; init; } = Array.Empty<TenantOption>();
    public IReadOnlyList<UserOption> Users { get; init; } = Array.Empty<UserOption>();
    public IReadOnlyList<AccountOption> Accounts { get; init; } = Array.Empty<AccountOption>();

    public static IValidator<ScenarioOption> Validator { get; } = new Validator<ScenarioOption>()
        .RuleFor(x => x.Subscriptions).NotNull()
        .RuleForEach(x => x.Subscriptions).Validate(SubscriptionOption.Validator)
        .RuleForEach(x => x.Tenants).Validate(TenantOption.Validator)
        .RuleFor(x => x.Users).NotNull()
        .RuleForEach(x => x.Users).Validate(UserOption.Validator)
        .RuleForEach(x => x.Accounts).Validate(AccountOption.Validator)
        .Build();

}

internal static class ScenarioOptionExtensions
{
    public static Option Validate(this ScenarioOption subject) => ScenarioOption.Validator.Validate(subject).ToOptionStatus();
}

internal record SubscriptionOption
{
    public string Name { get; init; } = null!;
    public string ContactName { get; init; } = null!;
    public string Email { get; init; } = null!;

    public static IValidator<SubscriptionOption> Validator { get; } = new Validator<SubscriptionOption>()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidResourceId(ResourceType.Principal)
        .Build();
}

internal record TenantOption
{
    public string Subscription { get; init; } = null!;
    public string Domain { get; init; } = null!;
    public string ContactName { get; init; } = null!;
    public string Email { get; init; } = null!;

    public static IValidator<TenantOption> Validator { get; } = new Validator<TenantOption>()
        .RuleFor(x => x.Subscription).NotEmpty()
        .RuleFor(x => x.Domain).NotEmpty()
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).NotEmpty()
        .Build();
}

internal record UserOption
{
    public string UserId { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;

    public static IValidator<UserOption> Validator { get; } = new Validator<UserOption>()
        .RuleFor(x => x.UserId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .Build();
}

internal record AccountOption
{
    public string AccountId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public string? WriteAccess { get; init; } = null!;
    public IReadOnlyList<LedgerItem> LedgerItems { get; init; } = Array.Empty<LedgerItem>();

    public static IValidator<AccountOption> Validator { get; } = new Validator<AccountOption>()
        .RuleFor(x => x.AccountId).ValidAccountId()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.WriteAccess).Must(x => x.IsEmpty() || x.Split(';').All(y => IdPatterns.IsPrincipalId(y)), x => $"Not valid principalIds: {x}")
        .RuleForEach(x => x.LedgerItems).Validate(LedgerItemValidator.Validator)
        .Build();
}

