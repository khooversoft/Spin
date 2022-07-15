//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Abstractions;

//namespace Contract.sdk.Models;

//public record ContractBlkHeader
//{
//    public DateTimeOffset Date { get; init; }
//    public string PrincipleId { get; init; } = null!;

//    public Guid ContractId { get; init; } = Guid.NewGuid();
//    public string Name { get; init; } = null!;
//    public string DocumentId { get; init; } = null!;
//}

//public record ContractBlkGroup
//{
//    public Guid Id { get; set; }
//    public DateTimeOffset Date { get; init; }
//    public string PrincipleId { get; init; } = null!;

//    public IReadOnlyList<ContractBlkDataItem> DataItems { get; init; } = Array.Empty<ContractBlkDataItem>();
//}

//public record ContractBlkDataItem
//{
//    public Guid Id { get; set; }
//    public DateTimeOffset Date { get; init; }
//    public string Name { get; set; } = null!;
//    public string Value { get; set; } = null!;
//}
