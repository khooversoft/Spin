using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public class DatalakeStoreFactory : IDatalakeStoreFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public DatalakeStoreFactory(ILoggerFactory loggerFactory)
        {
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _loggerFactory = loggerFactory;
        }

        public IDatalakeStore? Create(DatalakeStoreOption option)
        {
            option.VerifyNotNull(nameof(option));

            return new DatalakeStore(option, _loggerFactory.CreateLogger<DatalakeStore>());
        }
    }
}