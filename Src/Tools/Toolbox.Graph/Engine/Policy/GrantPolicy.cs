using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

/// <summary>
/// Record the grant policy to grant principalIdentifier (user or group) a role(s) on the nameIdentifier (node or edge)
/// 
/// Format: {nameIdentifier}:{role}:{schema}:{principalIdentifier}
/// Example: "customerNumber:o:ni:user1orGroupName"  - grants user1 owner role on node identified by nameIdentifier
/// </summary>
public readonly struct GrantPolicy : IEquatable<GrantPolicy>
{
    public GrantPolicy(string nameIdentifier, RolePolicy role, string principalIdentifier)
    {
        NameIdentifier = nameIdentifier.NotEmpty();
        Role = role.Assert(x => x.GetRole() != RolePolicy.None, x => $"Invalid role={role}");
        PrincipalIdentifier = principalIdentifier.NotNull();
    }

    [JsonConstructor]
    public GrantPolicy(string nameIdentifier, int roleNumeric, string principalIdentifier)
    {
        NameIdentifier = nameIdentifier.NotEmpty();
        Role = Enum.Parse<RolePolicy>(roleNumeric.ToString(), ignoreCase: true).Assert(x => x != RolePolicy.None, x => $"Invalid role={roleNumeric}");
        PrincipalIdentifier = principalIdentifier.NotNull();
    }

    public string NameIdentifier { get; }  // node or edge identifier that is protected (PK)
    [JsonIgnore] public RolePolicy Role { get; }
    public string PrincipalIdentifier { get; } // user or group that is granted Role access to Named Identifier

    public int RoleNumeric => (int)Role;
    public string Encode() => $"{NameIdentifier}:{PolicyRoleTool.ToEncode(Role)}:{PrincipalIdentifier}";
    public static GrantPolicy Parse(string encoded) => GrantPolicyTool.Parse(encoded);

    public bool Equals(GrantPolicy other) =>
        PrincipalIdentifier == other.PrincipalIdentifier &&
        Role == other.Role &&
        NameIdentifier == other.NameIdentifier;

    public override bool Equals(object? obj) => obj is GrantPolicy other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Role, NameIdentifier.ToLowerInvariant());
    public override string ToString() => Encode();

    public static implicit operator string(GrantPolicy encoding) => encoding.Encode();

    public static IValidator<GrantPolicy> Validator { get; } = new Validator<GrantPolicy>()
        .RuleFor(x => x.NameIdentifier).NotEmpty()
        .RuleFor(x => x.Role).Must(x => x != RolePolicy.None, _ => "Role cannot be None")
        .RuleFor(x => x.NameIdentifier).NotEmpty()
        .Build();
}

public static class GrantPolicyTool
{
    public static Option Validate(this GrantPolicy subject) => GrantPolicy.Validator.Validate(subject).ToOptionStatus();

    public static GrantPolicy Parse(string encoded)
    {
        const string schemaErrorMsg = "Invalid format. Expected: {nameIdentifier}:{schema}:{role}:{principalIdentifier}";

        encoded.Assert(x => x.IsNotEmpty() && x.Length >= 6, "Encoded string cannot be empty or not encoded correctly");

        ReadOnlySpan<char> span = encoded.AsSpan();

        // Find all colon positions in one pass
        Span<int> colonPositions = stackalloc int[3];
        int colonCount = 0;

        for (int i = 0; i < span.Length && colonCount < 4; i++)
        {
            if (span[i] == ':')
            {
                if (colonCount >= 3) throw new ArgumentException(schemaErrorMsg);
                colonPositions[colonCount++] = i;
            }
        }

        // Validate we have exactly 3 colons
        colonCount.Assert(x => x == 3, schemaErrorMsg);

        // Extract parts using the colon positions
        ReadOnlySpan<char> nameIdentifier = span[..colonPositions[0]];
        ReadOnlySpan<char> schemaPart = span[(colonPositions[0] + 1)..colonPositions[1]];
        ReadOnlySpan<char> rolePart = span[(colonPositions[1] + 1)..colonPositions[2]];
        ReadOnlySpan<char> principalIdentifier = span[(colonPositions[2] + 1)..];

        // Validate parts are not empty
        nameIdentifier.IsEmpty.BeFalse("NameIdentifier cannot be empty");
        schemaPart.IsEmpty.BeFalse("Schema cannot be empty");
        rolePart.IsEmpty.BeFalse("Role cannot be empty");
        principalIdentifier.IsEmpty.BeFalse("PrincipalIdentifier cannot be empty");

        // Validate schema and role
        PolicyRoleTool.TryGetSchema(schemaPart, out var schema).BeTrue($"Invalid schema '{schemaPart.ToString()}'");
        PolicyRoleTool.TryGetSchema(rolePart, out var role).BeTrue($"Invalid role '{rolePart.ToString()}'");

        return new GrantPolicy(nameIdentifier.ToString(), schema | role, principalIdentifier.ToString());
    }

    public static Option<IReadOnlyList<string>> InPolicy(this IReadOnlyCollection<GrantPolicy> grantPolicies, AccessRequest securityRequest)
    {
        if (grantPolicies.Count == 0) return StatusCode.OK;

        var groupList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool nameIdentifierFound = false;
        foreach (var principal in grantPolicies)
        {
            // grants for the right name identifier
            if (principal.NameIdentifier != securityRequest.NameIdentifier) continue;
            nameIdentifierFound = true;

            var accessType = principal.Role.ToAccessType();
            if ((accessType & securityRequest.AccessType) == 0) continue;

            if ((principal.Role & RolePolicy.SecurityGroup) != 0)
            {
                groupList.Add(principal.PrincipalIdentifier);
                continue;
            }

            if (principal.PrincipalIdentifier == securityRequest.PrincipalIdentifier) return StatusCode.OK;
        }

        switch (groupList.Count, nameIdentifierFound)
        {
            case (0, false): return StatusCode.OK; // name identifier not found
            case (0, true): return StatusCode.Unauthorized; // name identifier found, but no access
        }

        var result = ImmutableArray.CreateRange(groupList);
        return new Option<IReadOnlyList<string>>(result, StatusCode.NotFound);
    }
}
