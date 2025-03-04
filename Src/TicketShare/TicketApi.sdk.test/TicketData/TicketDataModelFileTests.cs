using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace TicketApi.sdk.test.TicketData;

public class TicketDataModelFileTests
{
    private readonly ScopeContext _context;
    private readonly TicketDataClient _dataClient;
    private readonly IFileStore _filestore;

    public TicketDataModelFileTests(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddLogging(config => config.AddDebug().AddConsole().AddLambda(output.WriteLine))
            .AddInMemoryFileStore()
            .AddSingleton<TicketDataClient>()
            .BuildServiceProvider();

        var logger = services.GetRequiredService<ILogger<TicketDataModelFileTests>>();
        _context = new ScopeContext(logger);

        _filestore = services.GetRequiredService<IFileStore>();
        _dataClient = services.GetRequiredService<TicketDataClient>();
    }

    [Fact]
    public async Task ReadWriteEmpty()
    {
        await _filestore.Delete(TicketDataClient.Path, _context);

        var m1 = new TicketDataRecord();
        var setResult = await _dataClient.Set(m1, _context);
        setResult.IsOk().Should().BeTrue();

        var getResult = await _dataClient.Get(_context);
        getResult.IsOk().Should().BeTrue();

        var m2 = getResult.Return();

        (m1 == m2).Should().BeTrue();
    }

    [Fact]
    public async Task ReadWriteData()
    {
        await _filestore.Delete(TicketDataClient.Path, _context);

        var m1 = new TicketDataRecord
        {
            Attractions = [
                new AttractionRecord { Id = "a1", Name = "name1a", Url = "url", Locale = "us-en" },
                new AttractionRecord { Id = "a2", Name = "name2a" },
            ],
            Events = [
                new EventRecord { Id = "1b", Name = "name1b", Timezone = "tz1" },
                new EventRecord { Id = "2b", Name = "name2b", Timezone = "tz2" },
            ],
            Venues = [
                new VenueRecord { Id = "1c", Name = "name1c", City = "city1c" },
                new VenueRecord { Id = "2c", Name = "name2c", City = "city2c" },
            ],
        };

        var setResult = await _dataClient.Set(m1, _context);
        setResult.IsOk().Should().BeTrue();

        var getResult = await _dataClient.Get(_context);
        getResult.IsOk().Should().BeTrue();

        var m2 = getResult.Return();

        (m1 == m2).Should().BeTrue();
    }
}
