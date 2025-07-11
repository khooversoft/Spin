namespace TicketShare.sdk;

public record UserProfileEditModel
{
    public string Name { get; set; } = "";
}

public static class UserProfileEditModelExtensions
{
    public static UserProfileEditModel Clone(this UserProfileEditModel subject) => new UserProfileEditModel
    {
        Name = subject.Name,
    };
}
