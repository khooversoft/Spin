using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record ContractDocumentModel : IEnumerable<DataGroup>
{
    public DateTimeOffset Date { get; init; }
    public Guid ContractId { get; init; } = Guid.NewGuid();
    public string PrincipleId { get; set; } = null!;
    public string Name { get; init; } = null!;
    public DocumentId DocumentId { get; init; } = null!;
    public IReadOnlyList<DataGroup> Groups { get; init; } = Array.Empty<DataGroup>();
    public IList<DataGroup> NewGroups { get; init; } = new List<DataGroup>();

    public ContractDocumentModel Add(DataGroup dataGroup) => this.Action(_ => NewGroups.Add(dataGroup.Verify()));

    public IEnumerator<DataGroup> GetEnumerator() => Groups.Concat(NewGroups).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Groups.Concat(NewGroups).GetEnumerator();
}


public static class ContractDocumentModelExtensions
{
    public static ContractDocumentModel Verify(this ContractDocumentModel subject)
    {
        subject.NotNull();
        subject.PrincipleId.NotEmpty();
        subject.Name.NotEmpty();
        subject.DocumentId.NotNull();
        subject.Groups.NotNull().ForEach(x => x.Verify());

        return subject;
    }

    public static ContractBlkHeader ConvertTo(this ContractDocumentModel subject)
    {
        subject.NotNull();

        return new ContractBlkHeader
        {
            Date = subject.Date,
            ContractId = subject.ContractId,
            PrincipleId = subject.PrincipleId,
            Name = subject.Name,
            DocumentId = (string)subject.DocumentId,
        };
    }

    public static ContractDocumentModel ConvertTo(this ContractBlkHeader header, IEnumerable<ContractBlkGroup> groups)
    {
        header.NotNull();
        groups.NotNull();

        return new ContractDocumentModel
        {
            Date = header.Date,
            ContractId = header.ContractId,
            PrincipleId = header.PrincipleId,
            Name = header.Name,
            DocumentId = (DocumentId)header.DocumentId,
            Groups = groups.Select(x => x.ConvertTo()).ToList(),
        };
    }
}