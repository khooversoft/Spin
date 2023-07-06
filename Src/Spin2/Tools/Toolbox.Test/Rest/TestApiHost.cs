using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Toolbox.TestApi;

namespace Toolbox.Test.Rest;

public class TestApiHost : WebApplicationFactory<TestApiServer>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        return base.CreateHost(builder);
    }
}

