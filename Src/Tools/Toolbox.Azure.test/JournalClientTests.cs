using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Data;
using Toolbox.Test.Data.Client.Common;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Azure.test;

public class JournalClientTests
{
    private readonly ITestOutputHelper _outputHelper;
    public JournalClientTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task SingleAppend()
    {
        using var host = BuildService();
        await DataJournalCommonTests.SingleAppend(host);
    }

    [Fact]
    public async Task TwoAppend()
    {
        using var host = BuildService();
        await DataJournalCommonTests.TwoAppend(host);
    }

    [Fact]
    public async Task ScaleAppend()
    {
        using var host = BuildService();
        await DataJournalCommonTests.ScaleAppend(host);
    }

    private IHost BuildService()
    {
        var datalakeOption = TestApplication.ReadOption("datastore-hybridCache-tests");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                services.AddDatalakeFileStore(datalakeOption);

                services.AddJournalPipeline<DataJournalCommonTests.EntityModel>(builder =>
                {
                    builder.AddMemory();
                    builder.AddJournalStore();
                });
            })
            .Build();

        return host;
    }
}
