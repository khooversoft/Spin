using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public sealed record SecurityGroupRecord
{
    public string SecurityGroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public IReadOnlyDictionary<string, MemberAccessRecord> Members { get; init; } = FrozenDictionary<string, MemberAccessRecord>.Empty;

    public bool Equals(SecurityGroupRecord? other) =>
        other is SecurityGroupRecord subject &&
        SecurityGroupId == other.SecurityGroupId &&
        Name == other.Name &&
        Members.DeepEquals(subject.Members);

    public override int GetHashCode() => HashCode.Combine(SecurityGroupId, Name, Members);

    public static IValidator<SecurityGroupRecord> Validator => new Validator<SecurityGroupRecord>()
        .RuleFor(x => x.SecurityGroupId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleForEach(x => x.Members.Values).Validate(MemberAccessRecord.Validator)
        .Build();

    public static SecurityGroupRecord Create(string securityGroupId, string name) => new SecurityGroupRecord
    {
        SecurityGroupId = securityGroupId.NotEmpty(),
        Name = name.NotEmpty(),
    };
}

public record MemberAccessRecord
{
    public string PrincipalId { get; init; } = null!;
    public PrincipalAccess Access { get; init; } = PrincipalAccess.None;

    public static IValidator<MemberAccessRecord> Validator => new Validator<MemberAccessRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Access).ValidEnum().Must(x => x != PrincipalAccess.None, _ => "None is not allowed")
        .Build();
}

public static class SecurityGroupRecordTool
{
    public static Option Validate(this SecurityGroupRecord subject) => SecurityGroupRecord.Validator.Validate(subject).ToOptionStatus();

    public static Option<string> CreateQuery(this SecurityGroupRecord subject, bool useSet, ScopeContext context)
    {
        if (subject.Validate().IsError(out var r)) return r.LogStatus(context, nameof(SecurityGroupClient)).ToOptionStatus<string>();

        string nodeKey = SecurityGroupTool.ToNodeKey(subject.SecurityGroupId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(SecurityGroupTool.NodeTag)
            .AddData("entity", subject)
            .AddReferences(
                SecurityGroupTool.EdgeType,
                subject.Members.Values.Select(x => GraphTool.ApplyIfRequired(x.PrincipalId, IdentityTool.ToNodeKey))
                )
            .Build();

        return cmd;
    }
}
