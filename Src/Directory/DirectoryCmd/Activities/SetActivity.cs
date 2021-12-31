using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace DirectoryCmd.Activities;

internal class SetActivity
{
    private readonly DirectoryClient _directoryClient;
    private readonly ILogger<SetActivity> _logger;

    public SetActivity(DirectoryClient directoryClient, ILogger<SetActivity> logger)
    {
        _directoryClient = directoryClient;
        _logger = logger;
    }

    public async Task SetFile(string file, CancellationToken token)
    {
        file.VerifyNotEmpty(nameof(file));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(SetFile), File = file });

        _logger.LogInformation($"Writing {file} to directory");

        string json = File.ReadAllText(file);

        IList<DirectoryEntry> list = Json.Default.Deserialize<IList<DirectoryEntry>>(json)
            .VerifyNotNull($"Cannot read json file {file}");

        int count = 0;
        foreach(var entry in list)
        {
            await _directoryClient.Set(entry, token);
            count++;
            _logger.LogInformation($"Writing {entry.DirectoryId}");
        }

        _logger.LogInformation($"Completed writing {count} entries from {file} to directory");
    }

    public async Task SetProperty(string directoryId, string[] properties, CancellationToken token)
    {
        DirectoryId id = new DirectoryId(directoryId);

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(SetProperty), DirectoryId = directoryId, Properties = properties });

        DirectoryEntry entry = (await _directoryClient.Get(id)) ?? new DirectoryEntry {  DirectoryId = (string)id };

        foreach(var item in properties)
        {
            (string key, string value) = GetProperty(item);
            entry.Properties[key] = new EntryProperty
            {
                Name = key,
                Value = value,
            };
        }

        await _directoryClient.Set(entry, token);

        (string key, string value) GetProperty(string value)
        {
            string[] values = value.Split('=')
                .Select(x => x.Trim())
                .ToArray()
                .VerifyAssert(x => x.Length == 2, $"{value} syntax error");

            return (values[0], values[1]);
        }
    }
}
