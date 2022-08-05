using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Extensions;

public static class TypeExtensions
{
    public static string GetTypeName(this Type type)
    {
        return type switch
        {
            Type t when t.IsGenericType => t.Name + ":" + t.GetGenericArguments().Select(x => x.Name).Join(","),
            _ => type.Name,
        };
    }

    public static string GetTypeName<T>(this T _) => typeof(T).GetTypeName();
}
