using Toolbox.Extensions;
using Toolbox.Types;

namespace NBlog.sdk;

internal static class ConfigurationFile
{
    public static Option<IReadOnlyList<string>> Read(string basePath, ScopeContext context)
    {
        string configurationFolder = Path.Combine(basePath, "Configuration");
        string search = "*" + NBlogConstants.ConfigurationExtension;
        string[] files = Directory.GetFiles(configurationFolder, search);

        if (files.Length == 0)
        {
            context.LogError("Cannot find any configuration files with folder={folder}, search={sarch}", configurationFolder, search);
            return (StatusCode.NotFound, "No configuration files");
        }

        var list = new Sequence<string>();
        foreach (var file in files)
        {
            var result = ReadAndVerify(file, context);
            if (result.IsError()) return (StatusCode.Conflict, $"Cannot read {file}");

            context.LogInformation("Read configuration file={file}", file);
            list += result.Return();
        }

        context.LogInformation("Read count={count} configuration files", list.Count);
        return list;
    }

    private static Option<string> ReadAndVerify(string file, ScopeContext context)
    {
        if (!File.Exists(file))
        {
            context.LogError("Cannot find configuration file={file}", file);
            return StatusCode.NotFound;
        }

        string configJson = File.ReadAllText(file);
        if (configJson.IsEmpty())
        {
            context.LogError("Configuration file={file} is empty", file);
            return StatusCode.NotFound;
        }

        var config = configJson.ToObject<NBlogConfiguration>();
        if (config == null)
        {
            context.LogError("Failed to deserialize configuration file={file}", file);
            return StatusCode.BadRequest;
        }

        if (!config.Validate(out Option v))
        {
            context.LogError("Configuration file={file} is invalid, error={error}", file, v.ToString());
            return StatusCode.BadRequest;
        }

        return file;
    }
}
