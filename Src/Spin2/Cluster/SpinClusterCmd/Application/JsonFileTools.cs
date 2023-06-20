using Toolbox.Tools;

namespace SpinClusterCmd.Application;

internal static class JsonFileTools
{
    public static T Read<T>(string file)
    {
        file.NotEmpty();
        string subject = File.ReadAllText(file);

        return Json.Default.Deserialize<T>(subject).NotNull();
    }
}
