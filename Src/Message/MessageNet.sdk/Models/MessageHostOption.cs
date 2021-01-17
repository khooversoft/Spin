using System.Collections.Generic;

namespace MessageNet.sdk.Models
{
    public record MessageHostOption
    {
        public IReadOnlyList<MessageNodeOption> Nodes { get; init; } = null!;
    }
}