using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artifact.sdk.Model
{
    public record ArtifactPayload
    {
        public string Id { get; init; } = null!;

        public string PackagePayload { get; init; } = null!;

        public string Hash { get; init; } = null!;
    }
}
