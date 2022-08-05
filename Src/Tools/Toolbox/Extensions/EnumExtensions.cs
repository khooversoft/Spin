using System;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class EnumExtensions
{
    public static T ToEnum<T>(this string value) where T : Enum
    {
        value.NotEmpty();

        Enum.TryParse(typeof(T), value, out object? enumValue)
            .Assert(x => x == true, $"{value} is not valid for enum {typeof(T).GetTypeName()}");

        return (T)enumValue!;
    }

    public static bool IsValid<T>(this string value) where T : Enum
    {
        value.NotEmpty();

        return Enum.IsDefined(typeof(T), value);
    }

    public static bool IsValid<T>(this T value) where T : struct, Enum => Enum.IsDefined<T>(value);
}
