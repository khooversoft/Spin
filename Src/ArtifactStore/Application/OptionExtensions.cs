using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace ArtifactStore.Application
{
    public static class OptionExtensions
    {
        public static void Verify(this Option option)
        {
            option.VerifyNotNull(nameof(option));

            option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");
            option.Store.Verify();
        }
    }
}
