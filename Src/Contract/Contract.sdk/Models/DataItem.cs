using Toolbox.Tools;

namespace Contract.sdk.Models;

public record DataItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Date { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}


public static class DataItemExtensions
{
    public static DataItem Verify(this DataItem subject)
    {
        subject.NotNull();
        subject.Name.NotEmpty();
        subject.Value.NotEmpty();

        return subject;
    }

    //public static ContractBlkDataItem ConvertTo(this DataItem subject)
    //{
    //    subject.NotNull();

    //    return new ContractBlkDataItem
    //    {
    //        Id = subject.Id,
    //        Date = subject.Date,
    //        Name = subject.Name,
    //        Value = subject.Value,
    //    };
    //}

    //public static DataItem ConvertTo(this ContractBlkDataItem subject)
    //{
    //    subject.NotNull();

    //    return new DataItem
    //    {
    //        Id = subject.Id,
    //        Date = subject.Date,
    //        Name = subject.Name,
    //        Value = subject.Value,
    //    };
    //}
}