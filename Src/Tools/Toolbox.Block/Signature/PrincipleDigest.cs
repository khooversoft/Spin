using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Block.Signature;

public record PrincipleDigest
{
    public string Key { get; init; } = Guid.NewGuid().ToString();

    public string PrincipleId { get; init; } = null!;

    public string Digest { get; init; } = null!;

    public string? JwtSignature { get; init; }
}
