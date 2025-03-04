using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TicketApi.sdk.MasterList;
using Toolbox;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace TicketApi.sdk.test;

public class TicketDataTests
{
    private readonly ScopeContext _context;
    private readonly IFileStore _filestore;
    private readonly TicketDataManager _manager;

    public TicketDataTests(ITestOutputHelper output)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("Application/test-appsettings.json")
            .AddUserSecrets("TicketApi.sdk.test-c1184340-447c-4c3d-8a49-09d46b80b30b")
            .Build();

        var ticketOption = config.GetSection("Ticket")
            .Get<TicketOption>().NotNull()
            .Action(x => x.Validate().ThrowOnError(nameof(TicketOption)));

        var services = new ServiceCollection()
            .AddLogging(logging => logging.AddDebug().AddConsole().AddLambda(output.WriteLine))
            .AddSingleton(config)
            .AddLocalFileStore(new LocalFileStoreOption { BasePath = "/work" })
            .AddTicket(ticketOption)
            .BuildServiceProvider();

        var logger = services.GetRequiredService<ILogger<TicketDataTests>>();
        _context = new ScopeContext(logger);

        _manager = services.GetRequiredService<TicketDataManager>();
        _filestore = services.GetRequiredService<IFileStore>();
    }

    [Fact/*(Skip = "destructed")*/]
    public async Task BuildModel()
    {
        var deleteOption = await _manager.ClearData(_context);
        deleteOption.IsOk().Should().BeTrue();

        var readOption = await _manager.Build(_context);
        readOption.IsOk().Should().BeTrue();

        var exist = await _filestore.Exist(TicketDataClient.Path, _context);
        exist.IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task LoadTicketData()
    {
        var readOption = await _manager.Get(_context);
        readOption.IsOk().Should().BeTrue();
        var read = readOption.Return();

        var attractions = read.Attractions
            .Select(x => x.Name)
            .Partition(5)
            .Select(x => x.Join(", "))
            .ToArray();

        attractions.ForEach(x => _context.LogInformation("Attractions: {attractions}", x));

        var teamDetails = TeamMasterList.GetDetails();
        var leagues = read.GetLeagues();

        leagues
            .Select(x => $"League={x.League}, Team={x.Team.Name}")
            .Partition(3)
            .ForEach(x => _context.LogInformation(x.Join(", ")));

        foreach (var league in leagues)
        {
            var events = read.GetEvents(league.Team.Id);

            events
                .Select(x => $"Name={x.Name}, VenueId={x.VenueId}")
                .Partition(3)
                .ForEach(x => _context.LogInformation(x.Join(", ")));
        }
    }
}
