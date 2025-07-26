using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Models;

public static class ChangeOperation
{
    public const string Add = "add";
    public const string Delete = "delete";
    public const string Update = "update";
}

public static class ChangeSource
{
    public const string Node = "node";
    public const string Edge = "edge";
    public const string Data = "data";
}

public record DataChangeRecord
{
    public string TransactionId { get; init; } = null!;
    public IReadOnlyList<DataChangeEntry> Entries { get; init; } = Array.Empty<DataChangeEntry>();

    public static IValidator<DataChangeRecord> Validator { get; } = new Validator<DataChangeRecord>()
        .RuleFor(x => x.TransactionId).NotEmpty()
        .RuleFor(x => x.Entries).NotNull()
        .RuleForEach(x => x.Entries).Validate(DataChangeEntry.Validator)
        .RuleForObject(x => x).Must(x => x.Entries.All(y => x.TransactionId == y.TransactionId), _ => "All entries must have the same TransactionId")
        .Build();
}

public static class DataChangeRecordTool
{
    public static Option Validate(this DataChangeRecord subject) => DataChangeRecord.Validator.Validate(subject).ToOptionStatus();

    public static string? GetLastLogSequenceNumber(this DataChangeRecord subject)
    {
        if (subject.Entries.Count == 0) return null;

        var result = subject.Entries
            .Select(x => x.LogSequenceNumber)
            .OrderByDescending(x => x)
            .FirstOrDefault();

        return result;
    }
}


public record DataChangeEntry
{
    public string LogSequenceNumber { get; init; } = null!;
    public string TransactionId { get; init; } = null!;
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string TypeName { get; init; } = null!;
    public string SourceName { get; init; } = null!;
    public string ObjectId { get; init; } = null!;
    public string Action { get; init; } = null!;
    public DataETag? Before { get; init; }
    public DataETag? After { get; init; }

    public override string ToString() => 
        $"Lsn={LogSequenceNumber}, TranId={TransactionId}, Date={Date:o}, TypeName={TypeName}, SourceName={SourceName}, ObjectId={ObjectId}, Action={Action}";

    public static IValidator<DataChangeEntry> Validator { get; } = new Validator<DataChangeEntry>()
        .RuleFor(x => x.LogSequenceNumber).NotEmpty()
        .RuleFor(x => x.TransactionId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.TypeName).NotEmpty()
        .RuleFor(x => x.SourceName).NotEmpty()
        .RuleFor(x => x.ObjectId).NotEmpty()
        .RuleFor(x => x.Action).NotEmpty()
        .Build();
}
