using System.Collections.Immutable;
using TicketShare.sdk;
using Fluent = Microsoft.FluentUI.AspNetCore.Components;

namespace TicketShareWeb.Components.Pages.Ticket.Model;

public record RoleModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PrincipalId { get; set; } = null!;
    public string MemberRole { get; set; } = null!;
}


public static class RoleModelTool
{
    public static RoleModel Clone(this RoleModel subject) => new RoleModel
    {
        Id = subject.Id,
        PrincipalId = subject.PrincipalId,
        MemberRole = subject.MemberRole,
    };

    public static RoleModel ConvertTo(this RoleRecord subject) => new RoleModel
    {
        Id = subject.Id,
        PrincipalId = subject.PrincipalId,
        MemberRole = subject.MemberRole.ToString(),
    };

    public static RoleRecord ConvertTo(this RoleModel subject) => new RoleRecord
    {
        Id = subject.Id,
        PrincipalId = subject.PrincipalId,
        MemberRole = Enum.Parse<RoleType>(subject.MemberRole),
    };

    public static IReadOnlyList<Fluent.Option<string>> ValidRoleTypes = Enum.GetNames<RoleType>()
        .OrderBy(x => x)
        .Select(x => new Fluent.Option<string> { Value = x, Text = x })
        .ToImmutableArray();
}
