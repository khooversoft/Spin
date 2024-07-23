using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record SeasonTicketRecord : IEquatable<SeasonTicketRecord>
{
    public string SeasonTicketId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerPrincipalId { get; init; } = null!;
    public string? Tags { get; init; }
    public IReadOnlyList<Property> Properties { get; init; } = ImmutableArray<Property>.Empty;
    public IReadOnlyList<RoleRecord> Members { get; init; } = ImmutableArray<RoleRecord>.Empty;
    public IReadOnlyList<SeatRecord> Seats { get; init; } = ImmutableArray<SeatRecord>.Empty;
    public IReadOnlyList<ChangeLog> ChangeLogs { get; init; } = ImmutableArray<ChangeLog>.Empty;

    public bool Equals(SeasonTicketRecord? other) =>
        other != null &&
        SeasonTicketId.Equals(other.SeasonTicketId) &&
        Name.Equals(other.Name) &&
        ((Description == null && other.Description == null) || Description?.Equals(other.Description) == true) &&
        OwnerPrincipalId.Equals(other.OwnerPrincipalId) &&
        ((Tags == null && other.Tags == null) || Tags?.Equals(other.Tags) == true) &&
        Enumerable.SequenceEqual(Properties.OrderBy(x => x.Key), other.Properties.OrderBy(x => x.Key)) &&
        Enumerable.SequenceEqual(Members.OrderBy(x => x.PrincipalId), other.Members.OrderBy(x => x.PrincipalId)) &&
        Enumerable.SequenceEqual(Seats.OrderBy(x => x.SeatId), other.Seats.OrderBy(x => x.SeatId)) &&
        Enumerable.SequenceEqual(ChangeLogs.OrderBy(x => x.Date), other.ChangeLogs.OrderBy(x => x.Date));

    public override int GetHashCode() => HashCode.Combine(SeasonTicketId, Name, Description, OwnerPrincipalId, Tags);

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
        .Select(x => x.SeasonTicketId, x => SelectNodeCommand(x))
        .Reference(x => x.OwnerPrincipalId, x => IdentityTool.ToUserKey(x), TicketShareTool.SeasonTicketToIdentity)
        .ReferenceCollection(x => x.Members.Select(y => y.PrincipalId), x => IdentityTool.ToUserKey(x), TicketShareTool.SeasonTicketToIdentity)
        .Build();

    public static string SelectNodeCommand(string seasonTicketId) => GraphTool.SelectNodeCommand(TicketShareTool.ToSeasonTicketKey(seasonTicketId), "entity");

    public static string GetSeasonTicketsForUser(string principalId) =>
        $"select (key={IdentityTool.ToUserKey(principalId)}) -> [edgeType={TicketShareTool.SeasonTicketToIdentity}] -> (*);";
}


public static class PartnershipRecordExtensions
{
    public static Option Validate(this SeasonTicketRecord subject) => SeasonTicketRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SeasonTicketRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}