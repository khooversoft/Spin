using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Block;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Broker;


public interface IMessageConnector<T>
{
    Task<Option<SpinMessage<T>>> Send(SpinMessage<T> message);
}

public class MessageBroker
{
    private readonly ILogger<MessageBroker> _logger;

    public MessageBroker(ILogger<MessageBroker> logger)
    {
        _logger = logger.NotNull();
    }

    //public IMessageConnector<T> GetConnector<T>(ObjectId objectId, ScopeContext context)
    //{
    //}
}


public record SpinMessage<T>
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string SchemaName { get; init; } = null!;
    public string RequestPrincipalId { get; init; } = null!;
    public string FromObjectId { get; init; } = null!;
    public string ToObjectId { get; init; } = null!;
    public string TraceId { get; init; } = null!;
    public T Value { get; init; } = default!;
}

public static class SpinMessageExtensions
{
    public static IValidator<SpinMessage<T>> GetValidator<T>() => new Validator<SpinMessage<T>>()
        .RuleFor(x => x.SchemaName).ValidName()
        .Build();

    public static ValidatorResult Validate<T>(this SpinMessage<T> subject, ScopeContextLocation location) => GetValidator<T>()
        .Validate(subject)
        .LogResult(location);

    public static Option<ValidatorResult> IsValid<T>(this SpinMessage<T> subject, ScopeContextLocation location)
    {
        var result = Validate<T>(subject, location);
        return new Option<ValidatorResult>(result, result.IsValid ? StatusCode.OK : StatusCode.BadRequest, error: result.FormatErrors());
    }
}
