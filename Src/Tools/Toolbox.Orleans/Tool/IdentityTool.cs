using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Orleans;

public static class IdentityTool
{
    public static string ToUserKey(string id) => $"user:{id.NotEmpty().ToLower()}";
    public static string ToUserNameIndex(string userName) => $"userName:{userName.NotEmpty().ToLower()}";
    public static string ToEmailIndex(string userName) => $"userEmail:{userName.NotEmpty().ToLower()}";
    public static string ToLoginIndex(string provider, string providerKey) => $"logonProvider:{provider.NotEmpty().ToLower() + "/" + providerKey.NotEmpty().ToLower()}";
}
