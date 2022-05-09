using System;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class EnumExtensions
{
    public static T ToEnum<T>(this string value) where T : Enum
    {
        value.NotEmpty(nameof(value));

        Enum.TryParse(typeof(T), value, out object? enumValue)
            .Assert(x => x == true, $"{value} is not valid for enum {typeof(T).Name}");

        return (T)enumValue!;
    }

    public static bool IsValid<T>(this string value) where T : Enum
    {
        value.NotEmpty(nameof(value));

        return Enum.IsDefined(typeof(T), value);
    }

    public static bool IsValid<T>(this T value) where T : struct, Enum
    {
        return Enum.IsDefined<T>(value);
    }
}
