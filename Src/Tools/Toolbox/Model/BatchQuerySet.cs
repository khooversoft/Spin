using System.Collections.Generic;
using Toolbox.Models;

namespace Toolbox.Model
{
    public record BatchQuerySet<T>
    {
        public QueryParameter QueryParameter { get; init; } = null!;

        public int NextIndex { get; init; }

        public IReadOnlyList<T> Records { get; init; } = null!;

        public bool IsEndSignaled { get => Records.Count == 0; }
    }
}
