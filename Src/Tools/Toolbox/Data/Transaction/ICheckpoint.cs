using Toolbox.Types;

namespace Toolbox.Data;

public interface ICheckpoint
{
    Task<string> Checkpoint();
    Task<Option> Recovery(string json);
}
