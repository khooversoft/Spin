using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Client.Common;

public class JournalClientSetupTests
{
    [Fact]
    public void NoHandler()
    {
        using var host = BuildService();

        Verify.Throw<ArgumentException>(() => host.Services.GetJournalClient<DataJournalCommonTests.EntityModel>());
    }

    private IHost BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddJournalPipeline<DataJournalCommonTests.EntityModel>(builder => { });
            })
            .Build();

        return host;
    }
}
