using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public sealed record ChannelViewRecord
{
    public string ChannelId { get; init; } = null!;
    public IReadOnlyList<ChannelMessage> Messages { get; init; } = Array.Empty<ChannelMessage>();


    public static IValidator<ChannelViewRecord> Validator { get; } = new Validator<ChannelViewRecord>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.Messages).NotNull()
        .Build();
}

public static class ChannelViewRecordExtensions
{
    public static Option Validate(this ChannelViewRecord subject) => ChannelViewRecord.Validator.Validate(subject).ToOptionStatus();
}
