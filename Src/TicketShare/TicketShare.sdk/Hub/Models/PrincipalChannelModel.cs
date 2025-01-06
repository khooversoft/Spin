using Toolbox.Tools;

namespace TicketShare.sdk;

public record PrincipalChannelModel
{
    public string PrincipalId { get; set; } = null!;
    public ChannelRole Role { get; set; } = ChannelRole.Reader;
}

public static class PrincipalChannelModelExtensios
{
    public static PrincipalChannelModel ConvertTo(this PrincipalChannelRecord record)
    {
        record.NotNull();

        var model = new PrincipalChannelModel
        {
            PrincipalId = record.PrincipalId,
            Role = record.Role,
        };

        return model;
    }

    public static PrincipalChannelRecord ConvertTo(this PrincipalChannelModel model)
    {
        model.NotNull();

        var record = new PrincipalChannelRecord
        {
            PrincipalId = model.PrincipalId,
            Role = model.Role,
        };

        return record;
    }
}
