namespace ContractHost.sdk.Event;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EventNameAttribute : Attribute
{
    public EventNameAttribute(EventName eventName)
    {
        EventName = eventName;
    }

    public EventName EventName { get; }
}