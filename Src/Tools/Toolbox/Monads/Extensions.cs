using Toolbox.Extensions;

namespace Toolbox.Monads;

public static class Extensions
{
    public static Unit<T> Unit<T>(this T value) => new Unit<T>(value);
    public static Option<T> Option<T>(this T? value, bool hasValue = true) => new Option<T>(hasValue, value);
    public static Option<T> Option<T>(this (bool hasValue, T? value) value) => new Option<T>(value.hasValue, value.value);
    public static Seq<T> Seq<T>(this T value) => new Seq<T>(value);
    public static Seq<TRoot, T> Seq<TRoot, T>(this TRoot value) => new Seq<TRoot, T>(value);
    public static Seq<T> Seq<T>(this Unit<T> value) => new Seq<T>(value);
    public static Seq<TRoot, T> Seq<TRoot, T>(this Unit<TRoot> value) => new Seq<TRoot, T>(value);
}
