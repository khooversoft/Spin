using Toolbox.Types;

namespace Toolbox.Block.Container;

public record DataBlock
{
    public string BlockId { get; init; } = Guid.NewGuid().ToString();
    public long TimeStamp { get; init; } = UnixDate.UtcNow;
    public string BlockType { get; init; } = null!;
    public string ObjectClass { get; init; } = null!;
    public string Data { get; init; } = null!;
    public string PrincipleId { get; init; } = null!;
    public string? JwtSignature { get; init; }
    public string Digest { get; init; } = null!;
}
