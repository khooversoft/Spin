using Toolbox.Extensions;
using Toolbox.Types;

namespace NBlog.sdk;

internal static class ConfigurationFile
{
    public static Option<string> Read(string basePath, ScopeContext context)
    {
        string configurationFile = Path.Combine(basePath, "Configuration", NBlogConstants.ConfigurationActorKey);
        if (!File.Exists(configurationFile))
        {
            context.LogError("Cannot find configuration file={file}", configurationFile);
            return StatusCode.NotFound;
        }

        string configJson = File.ReadAllText(configurationFile);
        if (configJson.IsEmpty())
        {
            context.LogError("Configuration file={file} is empty", configurationFile);
            return StatusCode.NotFound;
        }

        var config = configJson.ToObject<NBlogConfiguration>();
        if (config == null)
        {
            context.LogError("Failed to deserialize configuration file={file}", configurationFile);
            return StatusCode.BadRequest;
        }

        if (!config.Validate(out Option v))
        {
            context.LogError("Configuration file={file} is invalid, error={error}", configurationFile, v.ToString());
            return StatusCode.BadRequest;
        }

        return configurationFile;
    }
}
