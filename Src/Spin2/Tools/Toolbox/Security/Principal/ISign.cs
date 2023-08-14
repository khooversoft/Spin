using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Security.Principal;

public interface ISign
{
    Task<Option<string>> SignDigest(string kid, string messageDigest, string traceId);
}
