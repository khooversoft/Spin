using Microsoft.Extensions.Logging;
using NBlog.sdk.Models;
using Orleans.Concurrency;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface IContactRequestActor : IGrainWithIntegerKey
{
    Task<Option> Write(ContactRequest contactRequest, string traceId);
}

[StatelessWorker]
public class ContactRequestActor : Grain, IContactRequestActor
{
    private readonly IDatalakeStore _datalakeStore;
    private readonly RandomTag _randomTag;
    private readonly IClusterClient _clusterClient;
    private ILogger<ContactRequestActor> _logger;

    public ContactRequestActor(IDatalakeStore datalakeStore, RandomTag randomTag, IClusterClient clusterClient, ILogger<ContactRequestActor> logger)
    {
        _datalakeStore = datalakeStore.NotNull();
        _randomTag = randomTag.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Write(ContactRequest contactRequest, string traceId)
    {
        if (!contactRequest.Validate(out Option v)) return v;
        var context = new ScopeContext(traceId, _logger);

        IProfanityFilterActor cleanActor = _clusterClient.GetProfanityFilterActor();

        var nameOption = await cleanActor.Filter(contactRequest.Name, context.TraceId);
        if (nameOption.IsError()) return nameOption.ToOptionStatus();

        var emailOption = await cleanActor.Filter(contactRequest.Email, context.TraceId);
        if (emailOption.IsError()) return emailOption.ToOptionStatus();

        var messageOption = await cleanActor.Filter(contactRequest.Message, context.TraceId);
        if (messageOption.IsError()) return messageOption.ToOptionStatus();

        contactRequest = contactRequest with
        {
            Name = nameOption.Return(),
            Email = emailOption.Return(),
            Message = messageOption.Return(),
        };

        var dataEtag = contactRequest.ToJsonSafe(context.Location()).ToBytes().Func(x => new DataETag(x));
        string filePath = NBlogConstants.Tool.CreateContactRequestFileId(_randomTag.Get(10));

        var result = await _datalakeStore.Write(filePath, dataEtag, true, context);
        result.LogStatus(context, "Writing contact request to datalake");
        return result.ToOptionStatus();
    }
}
