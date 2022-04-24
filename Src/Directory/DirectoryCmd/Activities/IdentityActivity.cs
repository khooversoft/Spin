using Directory.sdk.Client;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions;
using Toolbox.Tools;

namespace DirectoryCmd.Activities;

internal class IdentityActivity
{
    private readonly IdentityClient _identityClient;
    private readonly ILogger<SetActivity> _logger;

    public IdentityActivity(IdentityClient identityClient, ILogger<SetActivity> logger)
    {
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task Create(string directoryId, string issuer, CancellationToken token)
    {
        var request = new IdentityEntryRequest
        {
            DirectoryId = directoryId,
            Issuer = issuer,
        };

        bool success = await _identityClient.Create(request, token);
        if (!success)
        {
            _logger.LogError($"Failed to create identity entry for directoryId={directoryId}");
            return;
        }

        _logger.LogInformation($"Created identity entry for directoryId={directoryId}");
    }

    public async Task Delete(string directoryId, CancellationToken token)
    {
        await _identityClient.Delete((DocumentId)directoryId, token);
        _logger.LogInformation($"Deleted directoryId={directoryId}");
    }

    public async Task Get(string directoryId, string file, CancellationToken token)
    {
        IdentityEntry? entry = await _identityClient.Get((DocumentId)directoryId, token);
        if(entry == null)
        {
            _logger.LogError($"Identity {directoryId} does not exist");
            return;
        }

        string json = Json.Default.SerializeFormat(entry);
        File.WriteAllText(file, json);
        _logger.LogInformation($"Writing directoryId={directoryId} to file {file}");
    }

    public async Task Set(string file, CancellationToken token)
    {
        file.VerifyNotEmpty(nameof(file));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(Set), File = file });

        if( !File.Exists(file) )
        {
            _logger.LogError($"{file} does not exit");
            return;
        }

        _logger.LogInformation($"Reading {file} to directory");
        string json = File.ReadAllText(file);

        IdentityEntry entry = Json.Default.Deserialize<IdentityEntry>(json)!;
        await _identityClient.Set(entry, token);
    }
}
