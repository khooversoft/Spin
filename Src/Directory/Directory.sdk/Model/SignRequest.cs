using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directory.sdk.Model;

public record SignRequest
{
    public string DirectoryId { get; init; } = null!;

    public string Digest { get; init; } = null!;
}
