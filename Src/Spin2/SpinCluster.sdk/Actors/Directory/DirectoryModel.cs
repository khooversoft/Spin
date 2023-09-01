//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Extensions;
//using Toolbox.Tools.Validation;
//using Toolbox.Types;
//using Toolbox.Types.ID;

//namespace SpinCluster.sdk.Actors.Directory;


//[GenerateSerializer, Immutable]
//public sealed record class DirectoryModel
//{
//    [Id(0)] public string DirectoryId { get; init; } = null!;
//    [Id(1)] public IDictionary<string, DirectoryEntry> Directory { get; init; } = new Dictionary<string, DirectoryEntry>(StringComparer.OrdinalIgnoreCase);
//}

//public enum DirectoryType
//{
//    Schema = 1,
//    Agent = 2,
//    SmartC = 3
//}


//[GenerateSerializer, Immutable]
//public sealed record DirectoryEntry
//{
//    [Id(0)] public DirectoryType Type { get; init; }
//    [Id(1)] public string ResourceId { get; init; } = null!;
//    [Id(2)] public bool Active { get; init; }
//    [Id(3)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
//    [Id(4)] public DateTime? LastRunDate { get; init; }
//}


//[GenerateSerializer, Immutable]
//public sealed record DirectoryQuery
//{
//    [Id(0)] public DirectoryType? Type { get; init; }
//    [Id(1)] public string? Schema { get; init; }
//    [Id(2)] public string? Domain { get; init; }
//    [Id(3)] public bool? Active { get; init; }
//}


//public static class DirectoryEntryValidator
//{
//    public static IValidator<DirectoryEntry> Validator { get; } = new Validator<DirectoryEntry>()
//        .RuleFor(x => x.Type).Must(x => x.IsEnumValid(), _ => "Not valid DirectoryType")
//        .RuleFor(x => x.ResourceId).ValidResourceId()
//        .Build();

//    public static Option Validate(this DirectoryEntry subject) => Validator.Validate(subject).ToOptionStatus();

//    public static bool IsMatchQuery(this DirectoryQuery query, DirectoryEntry subject)
//    {
//        if (query == null || subject == null) return false;

//        if (query.Type != null && query.Type != subject.Type) return false;
//        if (query.Active != null && query.Active != subject.Active) return false;

//        var ids = ResourceId.Create(subject.ResourceId);
//        if (ids.IsError()) return false;
//        if (query.Schema.IsNotEmpty() && query.Schema != ids.Return().Schema) return false;
//        if (query.Domain.IsNotEmpty() && query.Domain != ids.Return().Domain) return false;

//        return true;
//    }
//}