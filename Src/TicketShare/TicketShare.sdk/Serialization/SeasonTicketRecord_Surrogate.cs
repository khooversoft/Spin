//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TicketShare.sdk.Serialization;

//[GenerateSerializer]
//public struct SeasonTicketRecord_Surrogate
//{
//    [Id(0)] public string SeasonTicketId;
//    [Id(1)] public string Name;
//    [Id(2)] public string? Description;
//    [Id(3)] public string OwnerPrincipalId;
//    [Id(4)] public string? Tags;
//    [Id(5)] public Property[] Properties;
//    [Id(6)] public RoleRecord[] Members;
//    [Id(7)] public SeatRecord[] Seats;
//    [Id(8)] public ChangeLog[] ChangeLogs;
//}


//[RegisterConverter]
//public sealed class SeasonTicketRecord_SurrogateConverter : IConverter<SeasonTicketRecord, SeasonTicketRecord_Surrogate>
//{
//    public SeasonTicketRecord ConvertFromSurrogate(in SeasonTicketRecord_Surrogate surrogate)
//    {
//        var result = new SeasonTicketRecord
//        {
//            SeasonTicketId = surrogate.SeasonTicketId,
//            Name = surrogate.Name,
//            Description = surrogate.Description,
//            OwnerPrincipalId = surrogate.OwnerPrincipalId,
//            Tags = surrogate.Tags,
//            Properties = surrogate.Properties.ToImmutableArray(),
//            Members = surrogate.Members.ToImmutableArray(),
//            Seats = surrogate.Seats.ToImmutableArray(),
//            ChangeLogs = surrogate.ChangeLogs.ToImmutableArray(),
//        };

//        return result;
//    }

//    public SeasonTicketRecord_Surrogate ConvertToSurrogate(in SeasonTicketRecord value) => new SeasonTicketRecord_Surrogate
//    {
//        SeasonTicketId = value.SeasonTicketId,
//        Name = value.Name,
//        Description = value.Description,
//        OwnerPrincipalId = value.OwnerPrincipalId,
//        Tags = value.Tags,
//        Properties = value.Properties.ToArray(),
//        Members = value.Members.ToArray(),
//        Seats = value.Seats.ToArray(),
//        ChangeLogs = value.ChangeLogs.ToArray(),
//    };
//}