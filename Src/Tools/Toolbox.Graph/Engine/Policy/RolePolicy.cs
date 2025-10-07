using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Graph;


[Flags]
public enum RolePolicy
{
    None,
    Reader = 0x1,
    Contributor = 0x2,
    Owner = 0x3,
    NameIdentifier = 0x10,
    SecurityGroup = 0x20,
}


/// <summary>
/// On node - ni:o:user1
/// User = "user1"
/// 
/// Request: r:user1 -> "Read" - any name identifier that matches use "reader" role as mask
/// </summary>
public static class PolicyRoleTool
{
    private const int _roleMask = 0x3;
    private const int _roleTypeMask = 0x30;

    public static RolePolicy GetRole(this RolePolicy role) => ((int)role & _roleMask) switch
    {
        0x1 => RolePolicy.Reader,
        0x2 => RolePolicy.Contributor,
        0x3 => RolePolicy.Owner,
        _ => throw new ArgumentException($"Invalid RolePolicy policy"),
    };

    public static RolePolicy GetRoleType(this RolePolicy role) => ((int)role & _roleTypeMask) switch
    {
        0x10 => RolePolicy.NameIdentifier,
        0x11 => RolePolicy.SecurityGroup,
        _ => throw new ArgumentException($"Invalid RolePolicy policy"),
    };

    public static AccessType ToAccessType(this RolePolicy role) => role.GetRole() switch
    {
        RolePolicy.Reader => AccessType.Get,
        RolePolicy.Contributor => AccessType.Get | AccessType.Create | AccessType.Update | AccessType.Delete,
        RolePolicy.Owner => AccessType.Get | AccessType.Create | AccessType.Update | AccessType.Delete | AccessType.AssignRole,
        _ => throw new ArgumentException($"Invalid RolePolicy policy"),
    };

    public static bool TryGetSchema(ReadOnlySpan<char> value, out RolePolicy schema)
    {
        schema = value switch
        {
            { Length: 1 } => value[0] switch
            {
                'o' => RolePolicy.Owner,
                'c' => RolePolicy.Contributor,
                'r' => RolePolicy.Reader,
                _ => RolePolicy.None,
            },
            { Length: 2 } => value[0] switch
            {
                's' when value[1] == 'g' => RolePolicy.SecurityGroup,
                'n' when value[1] == 'i' => RolePolicy.NameIdentifier,
                _ => RolePolicy.None,
            },

            _ => RolePolicy.None,
        };

        return schema != RolePolicy.None;
    }

    public static string ToEncode(this RolePolicy role)
    {
        Span<char> buffer = stackalloc char[4];

        buffer[0] = role.HasFlag(RolePolicy.Owner) ?
            'o' : role.HasFlag(RolePolicy.Contributor) ?
            'c' : role.HasFlag(RolePolicy.Reader) ?
            'r' : throw new InvalidDataException("No role");

        buffer[1] = ':';

        ReadOnlySpan<char> schema = role.HasFlag(RolePolicy.SecurityGroup) ?
            "sg" : role.HasFlag(RolePolicy.NameIdentifier) ?
            "ni" : throw new InvalidDataException("No schema");

        schema.CopyTo(buffer.Slice(2));

        return buffer.ToString();
    }
}
