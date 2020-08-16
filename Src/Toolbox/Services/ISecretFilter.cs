using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Services
{
    public interface ISecretFilter
    {
        string? FilterSecrets(string? data, string replaceSecretWith = "***");
    }
}
