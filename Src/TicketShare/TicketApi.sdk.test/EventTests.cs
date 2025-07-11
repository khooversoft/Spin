//using Microsoft.Extensions.DependencyInjection;
//using TicketApi.sdk.test.Application;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketApi.sdk.test;

//public class EventTests
//{
//    [Fact]
//    public async Task TestSearch()
//    {
//        using var testHost = TestClientHostTool.Create();
//        TicketMasterClient client = testHost.Services.GetRequiredService<TicketMasterClient>();
//        var context = testHost.GetContext<EventTests>();

//        var classificationOption = await client.GetClassifications(context);
//        classificationOption.IsOk().BeTrue();
//        var segment = classificationOption.Return().Segments.First(x => x.Name == "Sports");
//        var genre = segment.Genres.First(x => x.Name == "Football");
//        var subGenre = genre.SubGenres.First(x => x.Name == "NFL");

//        var result = await client.GetEvents(segment, genre, subGenre, context);
//        result.IsOk().BeTrue();
//        result.Return().NotNull().Events.Count.Assert(x => x > 1, _ => "No events");
//    }
//}