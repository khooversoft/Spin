using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Services
{
    public interface IPropertyResolver
    {
        string Resolve(string subject);
    }
}
