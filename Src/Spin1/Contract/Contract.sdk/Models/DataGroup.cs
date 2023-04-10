using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Tools;

namespace Contract.sdk.Models;

public record DataGroup<T> where T : class
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Date { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}


public static class DataGroupExtensions
{
    public static DataGroup<T> Verify<T>(this DataGroup<T> subject) where T : class
    {
        subject.NotNull();
        subject.Items.NotNull();

        return subject;
    }
}
