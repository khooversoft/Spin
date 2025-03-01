using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace TicketMasterApi.sdk.MasterList;

public static class TeamMasterList
{
    private static readonly IReadOnlyList<string> _files = new string[]
    {
        "AHL.txt",
        "MLB.txt",
        "NAHL.txt",
        "NBA.txt",
        "NFL.txt",
        "NHL.txt",
    }.ToImmutableArray();

    public static IReadOnlyList<TeamDetail> GetDetails()
    {
        var details = _files
            .Select(x => (league: getleague(x), resourceId: $"TicketMasterApi.sdk.MasterList.{x}"))
            .Select(x => (x.league, text: AssemblyResource.GetResourceString(x.resourceId, typeof(TeamMasterList))))
            .SelectMany(x => ParseDetail(x.league, x.text))
            .ToArray();

        return details;

        string getleague(string file) => file.Split('.') switch
        {
            { Length: 2 } v => v[0],
            _ => file,
        };
    }

    private static IReadOnlyList<TeamDetail> ParseDetail(string league, string text)
    {
        var lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var classifications = lines
            .Where(x => x.IsNotEmpty())
            .Select(x => parse(x))
            .OfType<TeamClassification>()
            .ToArray();

        var template = new TeamDetail
        {
            Segments = classifications.Where(x => x.Attribute.EqualsIgnoreCase("segment")).ToImmutableArray(),
            Genres = classifications.Where(x => x.Attribute.EqualsIgnoreCase("genre")).ToImmutableArray(),
            SubGenres = classifications.Where(x => x.Attribute.EqualsIgnoreCase("subGenre")).ToImmutableArray(),
        };

        template.Segments.Count.Assert(x => x > 0, "No Segments found");
        template.Genres.Count.Assert(x => x > 0, "No Genres found");
        template.SubGenres.Count.Assert(x => x > 0, "No SubGenres found");
        var coverageCount = classifications.Length - template.Segments.Count - template.Genres.Count - template.SubGenres.Count;
        coverageCount.Should().Be(0, "Not all lines parsed");

        var details = lines
            .Where(x => x.IsNotEmpty())
            .Where(x => parse(x) == null)
            .Select(x => template with { League = league, Name = x })
            .ToArray();

        return details;

        static TeamClassification? parse(string line)
        {
            var segs = line.Split(':', StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (segs.Length == 1) return null;

            var keyValue = segs[1].Split('=', StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (keyValue.Length == 0) return null;

            return new TeamClassification
            {
                Attribute = segs[0],
                Name = keyValue[0],
                Id = keyValue[1],
            };
        }
    }
}
