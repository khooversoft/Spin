using Toolbox.Extensions;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkTransaction : BlkBase
{
    public IReadOnlyList<BlkRecord> Records { get; init; } = new List<BlkRecord>();
}

public record BlkRecord : BlkBase
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime TrxDate { get; init; } = DateTime.UtcNow;

    public string TrxType { get; init; } = null!;

    public double Value { get; init; }

    public string? Note { get; init; }
}


public static class BlkRecordExtensions
{
    public static void Verify(this BlkRecord subject)
    {
        subject.VerifyBase();
        subject.TrxType.VerifyNotEmpty(nameof(subject.TrxType));
        subject.Note.VerifyNotEmpty(nameof(subject.Note));
    }

    public static void Verify(this BlkTransaction subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.Records.VerifyNotNull(nameof(subject.Records));
        subject.Records.ForEach(x => x.Verify());
    }
}