using System;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block.Serialization;

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
}
