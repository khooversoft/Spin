using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record DataGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Date { get; init; } = DateTimeOffset.UtcNow;
    public string PrincipleId { get; set; } = null!;
    public IReadOnlyList<DataItem> DataItems { get; set; } = Array.Empty<DataItem>();
}


public static class DataGroupExtensions
{
    public static DataGroup Verify(this DataGroup subject)
    {
        subject.NotNull();
        subject.PrincipleId.NotEmpty();
        subject.DataItems.NotNull().ForEach(x => x.Verify());

        return subject;
    }

    //public static ContractBlkGroup ConvertTo(this DataGroup subject)
    //{
    //    subject.NotNull();

    //    return new ContractBlkGroup
    //    {
    //        Id = subject.Id,
    //        Date = subject.Date,
    //        PrincipleId = subject.PrincipleId,
    //        DataItems = subject.DataItems.Select(x => x.ConvertTo()).ToList(),
    //    };
    //}

    //public static DataGroup ConvertTo(this ContractBlkGroup subject)
    //{
    //    subject.NotNull();

    //    return new DataGroup
    //    {
    //        Id = subject.Id,
    //        Date = subject.Date,
    //        PrincipleId = subject.PrincipleId,
    //        DataItems = subject.DataItems.Select(x => x.ConvertTo()).ToList(),
    //    };
    //}
}
