using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Test.Data.Client.Common;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Client;

public class JournalClientSetupTests
{
    [Fact]
    public void NoHandler()
    {
        using var host = BuildService();

        Verify.Throw<ArgumentException>(() => host.Services.GetJournalClient<JournalCommonTests.EntityModel>("pipelineName"));
    }

    private IHost BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddJournalPipeline<JournalCommonTests.EntityModel>(builder => builder.BasePath = "basePath", "pipelineName");
            })
            .Build();

        return host;
    }
}
