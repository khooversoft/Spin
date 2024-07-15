using System.Collections.Immutable;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
public record SeasonTicketRecord
{
    [Id(0)] public string SeasonTicketId { get; init; } = null!;
    [Id(1)] public string Name { get; init; } = null!;
    [Id(2)] public string? Description { get; init; }
    [Id(3)] public string OwnerPrincipalId { get; init; } = null!;
    [Id(4)] public string? Tags { get; init; }
    [Id(5)] public IReadOnlyList<Property> Properties { get; init; } = ImmutableArray<Property>.Empty;
    [Id(6)] public IReadOnlyList<RoleRecord> Members { get; init; } = ImmutableArray<RoleRecord>.Empty;
    [Id(7)] public IReadOnlyList<SeatRecord> Seats { get; init; } = ImmutableArray<SeatRecord>.Empty;
    [Id(8)] public IReadOnlyList<ChangeLog> ChangeLogs { get; init; } = ImmutableArray<ChangeLog>.Empty;

    public static IValidator<SeasonTicketRecord> Validator { get; } = new Validator<SeasonTicketRecord>()
        .RuleFor(x => x.SeasonTicketId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.OwnerPrincipalId).NotEmpty()
        .RuleForEach(x => x.Properties).Validate(Property.Validator)
        .RuleForEach(x => x.Members).Validate(RoleRecord.Validator)
        .RuleForEach(x => x.Seats).Validate(SeatRecord.Validator)
        .RuleForEach(x => x.ChangeLogs).Validate(ChangeLog.Validator)
        .Build();

    public static IGraphSchema<SeasonTicketRecord> Schema { get; } = new GraphSchemaBuilder<SeasonTicketRecord>()
        .Node(x => x.SeasonTicketId, x => TicketShareTool.ToSeasonTicketKey(x))
        .Reference(x => x.OwnerPrincipalId, x => IdentityTool.ToUserKey(x), TicketShareTool.SeasonTicketToIdentity())
        .Reference(x => x.Members.Select(y => y.PrincipalId), x => x.Select(y => IdentityTool.ToUserKey(y)), TicketShareTool.SeasonTicketToIdentity())
        .Build();

    //public IReadOnlyList<string> GetIndexKeys() => new string?[]
    //{
    //    UserName.IsNotEmpty() ? IdentityTool.ToUserNameIndex(UserName) : null,
    //    Email.IsNotEmpty() ? IdentityTool.ToEmailIndex(Email) : null,
    //    LoginProvider.IsNotEmpty() && ProviderKey.IsNotEmpty() ? IdentityTool.ToLoginIndex(LoginProvider, ProviderKey) : null
    //}.OfType<string>().ToArray();
}2


public static class PartnershipRecordExtensions
{
    public static Option Validate(this SeasonTicketRecord subject) => SeasonTicketRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SeasonTicketRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}