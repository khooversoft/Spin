using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class AssemblyResource
{
    public static string GetResourceString(string resourceId, Type type)
    {
        resourceId.NotEmpty();
        type.NotNull();

        using var stream = type.Assembly.GetManifestResourceStream(resourceId);
        stream.NotNull($"Cannot find resourceId={resourceId}");

        return stream.ReadStringStream();
    }

    public static byte[] GetResourceBytes(string resourceId, Type type)
    {
        resourceId.NotEmpty();
        type.NotNull();

        using var stream = type.Assembly.GetManifestResourceStream(resourceId);
        stream.NotNull($"Cannot find resourceId={resourceId}");

        byte[] byteArray = new byte[stream.Length];
        stream.Read(byteArray, 0, byteArray.Length);

        return byteArray;
    }
}
