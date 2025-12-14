using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public record KeySystem : KeySystemBase, IKeySystem
{
    public KeySystem(string basePath) : base(basePath, KeySystemType.Key) { }

    public string PathBuilder(string key) => $"{this.CreatePathPrefix()}/{key.NotEmpty()}".ToLowerInvariant();

    public string PathBuilder<T>(string key)
    {
        key.NotEmpty();
        var typeName = typeof(T).Name;
        var result = $"{CreatePathPrefix()}/{typeName}/{key}.{typeName}.json".ToLowerInvariant();
        return result.ToLowerInvariant();
    }
}
