using Toolbox.Extensions;

namespace Toolbox.Monads;

public static class Extensions
{
    public static Unit<T> Unit<T>(this T value) => new Unit<T>(value);
    public static Maybe<T> Maybe<T>(this T value) => new Maybe<T>(value);
    public static Seq<T> Seq<T>(this T value) => new Seq<T>(value);
    public static Seq<TRoot, T> Seq<TRoot, T>(this TRoot value) => new Seq<TRoot, T>(value);
    public static Seq<T> Seq<T>(this Unit<T> value) => new Seq<T>(value);
    public static Seq<TRoot, T> Seq<TRoot, T>(this Unit<TRoot> value) => new Seq<TRoot, T>(value);
}
