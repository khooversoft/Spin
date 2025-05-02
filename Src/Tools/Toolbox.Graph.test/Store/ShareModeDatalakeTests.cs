using Toolbox.Graph.test.Application;
using Toolbox.Graph.test.Store.TestingCode;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Store;

public class ShareModeDatalakeTests
{
    private const string _basePath = $"graphTesting-{nameof(ShareModeDatalakeTests)}";
    private readonly ITestOutputHelper _outputHelper;
    public ShareModeDatalakeTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task OneWriteOtherRead()
    {

        var (firstClient, secondClient, context) = await TestApplication.CreateTwoLinkClientsForDatalake<ShareModeDatalakeTests>(_basePath, _outputHelper);
        using (firstClient)
        using (secondClient)
        {
            await ShareModeTesting.OneWriteOtherRead(firstClient, secondClient, context);
        }
    }


    [Fact]
    public async Task ParallelReads()
    {
        var (firstClient, secondClient, context) = await TestApplication.CreateTwoLinkClientsForDatalake<ShareModeDatalakeTests>(_basePath, _outputHelper);
        using (firstClient)
        using (secondClient)
        {
            await ShareModeTesting.ParallelReads(firstClient, secondClient, context);
        }
    }
}
