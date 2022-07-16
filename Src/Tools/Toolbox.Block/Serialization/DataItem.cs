using System;
using System.Diagnostics;
using Toolbox.Tools;

namespace Toolbox.Block.Serialization;

[DebuggerDisplay("Id={Id}, Index={Index}, DataType={DataType}, Key={Key}, Value={Value}")]
public record DataItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    public int Index { get; set; }
    public string DataType { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}


public static class DataItemExtensions
{
    public static DataItem Verify(this DataItem subject)
    {
        subject.NotNull();
        subject.DataType.NotEmpty();
        subject.Key.NotEmpty();
        subject.Value.NotEmpty();

        return subject;
    }
}