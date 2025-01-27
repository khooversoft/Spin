namespace Toolbox.Graph.Extensions;

[Flags]
public enum PrincipalAccess
{
    None = 0,
    Read = 0x1,
    Contributor = 0x2,
    Owner = 0x4,
}
