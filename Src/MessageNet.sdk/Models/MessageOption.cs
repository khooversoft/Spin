using System.Collections.Generic;

namespace MessageNet.sdk.Models
{
    public record MessageOption
    {
        public IReadOnlyList<MessageNodeOption> Nodes { get; init; } = null!;
    }
}