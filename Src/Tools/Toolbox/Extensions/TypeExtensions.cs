namespace Toolbox.Extensions;

public static class TypeExtensions
{
    public static string GetTypeName(this Type type)
    {
        return type switch
        {
            Type t when t.IsGenericType => t.Name + "_" + t.GetGenericArguments().Select(x => x.Name).Join("_"),
            _ => type.Name,
        };
    }

    public static string GetTypeName<T>(this T _) => typeof(T).GetTypeName();
}
