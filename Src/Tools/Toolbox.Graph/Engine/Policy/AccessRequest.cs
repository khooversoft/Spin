using Toolbox.Tools;

namespace Toolbox.Graph;


[Flags]
public enum AccessType
{
    None,
    Get = 0x10,
    Create = 0x20,
    Update = 0x40,
    Delete = 0x80,
    AssignRole = 0x100,
}

public readonly struct AccessRequest
{
    public AccessRequest(AccessType accessType, string principalIdentifier, string nameIdentifier)
    {
        AccessType = accessType;
        NameIdentifier = nameIdentifier.NotEmpty();
        PrincipalIdentifier = principalIdentifier.NotEmpty();
    }

    public AccessType AccessType { get; }
    public string PrincipalIdentifier { get; }
    public string NameIdentifier { get; }
}
