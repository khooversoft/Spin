using Directory.sdk;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DirectoryCmd.Activities;

internal class SetActivity
{
    private readonly DirectoryClient _directoryClient;
    private readonly IdentityClient _identityClient;
    private readonly ILogger<SetActivity> _logger;

    public SetActivity(DirectoryClient directoryClient, IdentityClient identityClient, ILogger<SetActivity> logger)
    {
        _directoryClient = directoryClient;
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task SetFile(string file, CancellationToken token)
    {
        file.VerifyNotEmpty(nameof(file));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(SetFile), File = file });

        _logger.LogInformation($"Reading {file} to directory");
        string json = File.ReadAllText(file);

        IList<DirectoryEntry> list = Json.Default.Deserialize<IList<DirectoryEntry>>(json)
            .VerifyNotNull($"Cannot read json file {file}");

        foreach (DirectoryEntry entry in list)
        {
            await _directoryClient.Set(entry, token);
            _logger.LogInformation($"Writing Id {entry.DirectoryId}");

            if (entry.ClassType == ClassTypeName.User)
            {
                await CreateIdentity(entry, token);
            }
        }

        _logger.LogInformation($"Completed writing {list.Count} entries from {file} to directory");
    }

    public async Task SetProperty(string directoryId, string[] properties, CancellationToken token)
    {
        var id = new DocumentId(directoryId);
        _logger.LogInformation($"Updating property {properties.Join(", ")} on {directoryId}");

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(SetProperty), DirectoryId = directoryId, Properties = properties });

        DirectoryEntry entry = (await _directoryClient.Get(id)) ?? new DirectoryEntry { DirectoryId = (string)id };

        foreach (var item in properties)
        {
            (string key, string value) = GetProperty(item);
            entry.Properties[key] = new EntryProperty
            {
                Name = key,
                Value = value,
            };
        }

        await _directoryClient.Set(entry, token);
        _logger.LogInformation($"Updated property {properties.Join(", ")} on {directoryId}");

        (string key, string value) GetProperty(string value)
        {
            string[] values = value.Split('=')
                .Select(x => x.Trim())
                .ToArray()
                .VerifyAssert(x => x.Length == 2, $"{value} syntax error");

            return (values[0], values[1]);
        }
    }

    private async Task CreateIdentity(DirectoryEntry entry, CancellationToken token)
    {
        string? email = entry.GetEmail();
        string? identityId = entry.GetSigningCredentials();

        if (email == null && identityId == null) return;

        if (!(email != null && identityId != null))
        {
            _logger.LogError($"Directory Id {entry.DirectoryId} must specify both {PropertyName.Email} and {PropertyName.SigningCredentials} properties");
            return;
        }

        var identityEntryRequest = new IdentityEntryRequest
        {
            DirectoryId = identityId,
            Issuer = email,
        };

        await _identityClient.Create(identityEntryRequest, token);
    }
}
