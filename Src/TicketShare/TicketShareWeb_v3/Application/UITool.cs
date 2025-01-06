using System.Collections.Immutable;
using TicketShare.sdk;
using Fluent = Microsoft.FluentUI.AspNetCore.Components;

namespace TicketShareWeb.Application;

public static class UITool
{
    public static IReadOnlyList<Fluent.Option<string>> ToFluentOption<TEnum>() where TEnum : struct, Enum
    {
        var list = Enum.GetNames<TEnum>()
           .OrderBy(x => x)
           .Select(x => new Fluent.Option<string> { Value = x, Text = x })
           .ToImmutableArray();

        return list;
    }

    public static IReadOnlyList<Fluent.Option<string>> ValidRoleTypes { get; } = ToFluentOption<RoleType>();

    public static IReadOnlyList<Fluent.Option<string>> ValidContactTypes = ToFluentOption<ContactType>();
}
