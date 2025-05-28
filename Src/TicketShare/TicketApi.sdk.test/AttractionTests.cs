////using TicketMasterApi.sdk.Model;
//using TicketApi.sdk.test.Application;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketApi.sdk.test;

//public class AttractionTests
//{
//    [Fact]
//    public async Task TestSearch()
//    {
//        var testHost = TestClientHostTool.Create();
//        TicketAttractionClient client = testHost.GetAttractionClient();
//        var context = testHost.GetContext<AttractionTests>();

//        IDictionary<string, TeamDetail> teamDetails = TeamMasterList.GetDetails();
//        var sequence = new Sequence<AttractionRecord>();
//        var notFound = new Sequence<string>();
//        var errors = new Sequence<string>();

//        var result = await client.GetAttractions(teamDetails.Values, context);
//        result.IsOk().BeTrue();
//        result.Return().NotNull();
//        result.Return().Attractions.Count.Assert(x => x > 10, _ => "Empty list");
//        result.Return().Images.Count.Assert(x => x > 10, _ => "Empty list");

//        //foreach (var team in teamDetails)
//        //{
//        //    if (result.IsError())
//        //    {
//        //        errors += $"Team={team.Name}, error={result.Error}";
//        //        continue;
//        //    }

//        //    var data = result.Return().NotNull();
//        //    if (data.Count == 0)
//        //    {
//        //        notFound += team.Name;
//        //        continue;
//        //    }

//        //    sequence += data;
//        //    page++;
//        //}

//        //var count = sequence.Count;

//        //var result = await client.GetAttractions(context);
//        //result.IsOk().BeTrue();
//        //result.Return().NotNull();
//        //result.Return().Count.Assert(x => x > 10, _ => "Empty list");
//    }
//}
