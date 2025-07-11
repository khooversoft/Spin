//using System.Collections.Frozen;

//namespace TicketApi.sdk;

//public static partial class TeamMasterList
//{
//    private static readonly FrozenDictionary<string, TeamDetail> _teamDetails = InternalGetDetails().ToFrozenDictionary(x => x.Name);
//    private static IReadOnlyList<TeamDetail> InternalGetDetails() => [AHL, MLB, NAHL, NBA, NFL, NHL];

//    public static IDictionary<string, TeamDetail> GetDetails() => _teamDetails;
//}
