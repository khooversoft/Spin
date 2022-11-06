using System;
using System.Linq;
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

    public static string? FindEnumValue<T>(this string value, bool caseInsensitive = false) where T : Enum
    {
        value.NotEmpty();

        return caseInsensitive switch
        {
            false => Enum.GetNames(typeof(T)).FirstOrDefault(x => x == value),
            true => Enum.GetNames(typeof(T)).FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)),
        };
    }

    public static bool IsValid<T>(this T value) where T : struct, Enum => Enum.IsDefined<T>(value);
}
