using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public sealed record SecurityGroupRecord
{
    public string SecurityGroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public IReadOnlyDictionary<string, PrincipalAccess> Members { get; init; } = FrozenDictionary<string, PrincipalAccess>.Empty;

    public bool Equals(SecurityGroupRecord? other) =>
        other is SecurityGroupRecord subject &&
        SecurityGroupId == other.SecurityGroupId &&
        Name == other.Name &&
        Members.DeepEquals(subject.Members);

    public override int GetHashCode() => HashCode.Combine(SecurityGroupId, Name, Members);

    public static IValidator<SecurityGroupRecord> Validator => new Validator<SecurityGroupRecord>()
        .RuleFor(x => x.SecurityGroupId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Members).Must(x => x.Count > 0, _ => "Must have access")
        .RuleForEach(x => x.Members.Values).Validate(PrincipalAccess.Validator)
        .RuleFor(x => x.Members.Values).Must(x => x.Any(y => y.Access == SecurityAccess.Owner), _ => "Must have owner")
        .Build();

    public static SecurityGroupRecord Create(string securityGroupId, string name) => new SecurityGroupRecord
    {
        SecurityGroupId = securityGroupId.NotEmpty(),
        Name = name.NotEmpty(),
    };
}

public static class SecurityGroupRecordTool
{
    public static Option Validate(this SecurityGroupRecord subject) => SecurityGroupRecord.Validator.Validate(subject).ToOptionStatus();


    public static Option HasAccess(this SecurityGroupRecord subject, string principalId, SecurityAccess access)
    {
        var result = subject.Members.TryGetValue(principalId, out var accessRecord) switch
        {
            true => accessRecord.HasAccess(access),
            false => StatusCode.Unauthorized,
        };

        return result;
    }
}
