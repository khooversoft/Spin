﻿using Microsoft.Azure.Amqp.Framing;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

[GenerateSerializer, Immutable]
public sealed record TenantModel
{
    private const string _version = nameof(TenantModel) + "-v1";

    [Id(0)] public string TenantId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();
    [Id(3)] public string Name { get; init; } = null!;
    [Id(4)] public string SubscriptionName { get; init; } = null!;
    [Id(5)] public string ContactName { get; init; } = null!;
    [Id(6)] public string Email { get; init; } = null!;
    [Id(7)] public IReadOnlyList<UserPhoneModel> Phone { get; init; } = Array.Empty<UserPhoneModel>();
    [Id(8)] public IReadOnlyList<UserAddressModel> Address { get; init; } = Array.Empty<UserAddressModel>();
    [Id(9)] public bool AccountEnabled { get; init; } = false;
    [Id(10)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(11)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(TenantModel? obj) => obj is TenantModel document &&
        TenantId == document.TenantId &&
        Version == document.Version &&
        GlobalId == document.GlobalId &&
        Name == document.Name &&
        SubscriptionName == document.SubscriptionName &&
        ContactName == document.ContactName &&
        Email == document.Email &&
        Phone.SequenceEqual(document.Phone) &&
        Address.SequenceEqual(document.Address) &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(TenantId, GlobalId, Name, ContactName);

    public static ObjectId CreateId(NameId nameId) => $"{SpinConstants.Schema.Tenant}/$system/{nameId}";
}


public static class TenantRegisterValidator
{
    public static IValidator<TenantModel> Validator { get; } = new Validator<TenantModel>()
        .RuleFor(x => x.TenantId).ValidObjectId()
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.SubscriptionName).ValidName()
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidPrincipalId()
        .RuleFor(x => x.Phone).NotNull()
        .RuleForEach(x => x.Phone).Validate(UserPhoneModelValidator.Validator)
        .RuleFor(x => x.Address).NotNull()
        .RuleForEach(x => x.Address).Validate(UserAddressModelValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this TenantModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}