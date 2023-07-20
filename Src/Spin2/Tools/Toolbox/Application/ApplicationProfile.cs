using System.Collections.Concurrent;

namespace Toolbox.Application;

public class ApplicationProfiles : ConcurrentDictionary<string, ApplicationProfile> { }

public record ApplicationProfile
{
    public string Domain { get; set; } = null!;
    public IDictionary<string, string> Map { get; init; } =
        new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
