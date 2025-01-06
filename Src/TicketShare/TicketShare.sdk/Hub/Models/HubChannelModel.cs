using System.Collections.Concurrent;
using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace TicketShare.sdk;

public class HubChannelModel
{
    public string ChannelId { get; set; } = null!;
    public string Name { get; init; } = null!;
    public ConcurrentDictionary<string, PrincipalChannelModel> Users = new(StringComparer.OrdinalIgnoreCase);
}

public static class HubChannelModelExtensions
{
    public static HubChannelModel ConvertTo(this HubChannelRecord subject)
    {
        subject.NotNull();

        var result = new HubChannelModel
        {
            ChannelId = subject.ChannelId,
            Name = subject.Name,
            Users = subject.Users.Values
                .Select(x => x.ConvertTo())
                .ToConcurrentDictionary(x => x.PrincipalId),
        };

        return result;
    }

    public static HubChannelRecord ConvertTo(this HubChannelModel subject)
    {
        subject.NotNull();
        var result = new HubChannelRecord
        {
            ChannelId = subject.ChannelId,
            Name = subject.Name,
            Users = subject.Users
                .Select(x => x.Value.ConvertTo())
                .ToFrozenDictionary(x => x.PrincipalId)
        };
        return result;
    }

    public static HubChannelRecord ConverToWith(this HubChannelModel subject, HubChannelRecord record)
    {
        subject.NotNull();
        record.NotNull();

        var mergedUsers = record.Users.ToDictionary();
        subject.Users.Values.ForEach(x => mergedUsers[x.PrincipalId] = x.ConvertTo());

        var result = record with
        {
            Name = subject.Name,
            Users = mergedUsers.ToFrozenDictionary(),
        };

        return result;
    }
}