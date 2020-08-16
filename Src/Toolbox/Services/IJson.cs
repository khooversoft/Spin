using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Services
{
    /// <summary>
    /// Interface for Json services
    /// </summary>
    public interface IJson
    {
        T Deserialize<T>(string subject);

        string Serialize<T>(T subject);

        string SerializeFormat<T>(T subject);
    }
}
