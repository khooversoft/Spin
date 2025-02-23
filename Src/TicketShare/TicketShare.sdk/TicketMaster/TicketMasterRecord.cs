using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Logging;
using Toolbox.Graph;

namespace TicketShare.sdk;

public record TicketMasterRecord
{
    public string TicketMasterId { get; init; } = null!;
    public IReadOnlyDictionary<string, Promoter> Promoters { get; init; } = FrozenDictionary<string, Promoter>.Empty;

    public static IValidator<TicketMasterRecord> Validator => new Validator<TicketMasterRecord>()
        .RuleFor(x => x.Promoters).NotNull()
        .RuleForEach(x => x.Promoters.Values).Validate(Promoter.Validator)
        .Build();
}

public static class TicketMasterRecordTool
{
    public const string NodeTag = "ticketMaster";
    public const string NodeKeyPrefix = "ticketMaster:";

    public static Option Validate(this TicketMasterRecord subject) => TicketMasterRecord.Validator.Validate(subject).ToOptionStatus();

    public static string ToNodeKey(string channelId) => $"{NodeKeyPrefix}{channelId.NotEmpty().ToLower()}";
    public static string CleanNodeKey(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix) switch
    {
        false => subject,
        true => subject[NodeKeyPrefix.Length..],
    };

    public static Option<string> CreateQuery(this TicketMasterRecord subject, ScopeContext context)
    {
        if (subject.Validate().IsError(out var r)) return r.LogStatus(context, nameof(TicketMasterRecord)).ToOptionStatus<string>();

        string nodeKey = ToNodeKey(subject.TicketMasterId);

        var cmd = new NodeCommandBuilder()
            .UseSet(true)
            .SetNodeKey(nodeKey)
            .AddTag(NodeTag)
            .AddData("entity", subject)
            .Build();

        return cmd;
    }
}


public record Promoter
{
    public string Name { get; init; } = null!;
    public string PromoterId { get; init; } = null!;

    public static IValidator<Promoter> Validator => new Validator<Promoter>()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.PromoterId).NotEmpty()
        .Build();
}
