using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
public record PartnershipRecord
{
    [Id(0)] public string Id { get; init; } = null!;
    [Id(1)] public string Name { get; init; } = null!;
    [Id(2)] public string? Description { get; init; }
    [Id(3)] public string OwnerPrincipalId { get; init; } = null!;
    [Id(4)] public string? Tags { get; init; }
    [Id(5)] public IReadOnlyList<Property> Properties { get; init; } = ImmutableArray<Property>.Empty;
    [Id(6)] public IReadOnlyList<RoleRecord> Members { get; init; } = ImmutableArray<RoleRecord>.Empty;
    [Id(7)] public IReadOnlyList<SeatRecord> Seats { get; init; } = ImmutableArray<SeatRecord>.Empty;
    [Id(8)] public IReadOnlyList<ChangeLog> ChangeLogs { get; init; } = ImmutableArray<ChangeLog>.Empty;

    public static IValidator<PartnershipRecord> Validator { get; } = new Validator<PartnershipRecord>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.OwnerPrincipalId).NotEmpty()
        .RuleForEach(x => x.Properties).Validate(Property.Validator)
        .RuleForEach(x => x.Members).Validate(RoleRecord.Validator)
        .RuleForEach(x => x.Seats).Validate(SeatRecord.Validator)
        .RuleForEach(x => x.ChangeLogs).Validate(ChangeLog.Validator)
        .Build();

    //public IReadOnlyList<string> GetIndexKeys() => new string?[]
    //{
    //    UserName.IsNotEmpty() ? IdentityTool.ToUserNameIndex(UserName) : null,
    //    Email.IsNotEmpty() ? IdentityTool.ToEmailIndex(Email) : null,
    //    LoginProvider.IsNotEmpty() && ProviderKey.IsNotEmpty() ? IdentityTool.ToLoginIndex(LoginProvider, ProviderKey) : null
    //}.OfType<string>().ToArray();
}


public static class PartnershipRecordExtensions
{
    public static Option Validate(this PartnershipRecord subject) => PartnershipRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this PartnershipRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}