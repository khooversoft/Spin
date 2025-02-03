//using Toolbox.Tools;

//namespace TicketShare.sdk;

//public record PrincipalRoleModel
//{
//    public string PrincipalId { get; set; } = null!;
//    public ChannelRole Role { get; set; } = ChannelRole.Reader;
//}

//public static class PrincipalChannelModelExtensios
//{
//    public static PrincipalRoleModel ConvertTo(this PrincipalRoleRecord record)
//    {
//        record.NotNull();

//        var model = new PrincipalRoleModel
//        {
//            PrincipalId = record.PrincipalId,
//            Role = record.Role,
//        };

//        return model;
//    }

//    public static PrincipalRoleRecord ConvertTo(this PrincipalRoleModel model)
//    {
//        model.NotNull();

//        var record = new PrincipalRoleRecord
//        {
//            PrincipalId = model.PrincipalId,
//            Role = model.Role,
//        };

//        return record;
//    }
//}
