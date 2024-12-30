namespace TicketShare.sdk;

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
}
