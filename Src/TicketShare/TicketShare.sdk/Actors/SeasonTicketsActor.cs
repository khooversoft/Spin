using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Actors;

public interface ISeasonTicketsActor : IGrainWithStringKey
{
    Task<Option> AddProperty(string seasonTicketId, Property addProperty, ScopeContext context);
    Task<Option> RemoveProperty(string seasonTicketId, string propertyKey, ScopeContext context);
    Task<Option> AddRole(string seasonTicketId, RoleRecord addRole, ScopeContext context);
    Task<Option> RemoveRole(string seasonTicketId, string principalId, ScopeContext context);
    Task<Option> AddSeat(string seasonTicketId, SeatRecord seatRecord, ScopeContext context);
    Task<Option> RemoveSeat(string seasonTicketId, string seatId, DateTime eventDate, ScopeContext context);
    Task<Option<SeasonTicketRecord>> Get(string patnershipId, ScopeContext context);
    Task<Option> Set(SeasonTicketRecord accountName, ScopeContext context);
}

[StatelessWorker]
public class SeasonTicketsActor : Grain, ISeasonTicketsActor
{
    private readonly ILogger<SeasonTicketsActor> _logger;
    private readonly IClusterClient _clusterClient;

    public SeasonTicketsActor(IClusterClient clusterClient, ILogger<SeasonTicketsActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> AddProperty(string seasonTicketId, Property addProperty, ScopeContext context)
    {
        var test = new OptionTest()
            .Test(() => seasonTicketId.IsNotEmpty(), error: $"{nameof(seasonTicketId)} is required")
            .Test(() => addProperty.Validate());
        if (test.IsError()) return test.Option;

        context = context.With(_logger);
        context.LogInformation("Adding property for seasonTicketId={seasonTicketId}, addProperty={addProperty}", seasonTicketId, addProperty);

        var ticketOption = await Get(seasonTicketId, context);
        if (ticketOption.IsError()) return ticketOption.LogStatus(context, "Failed to read").ToOptionStatus();

        var ticket = ticketOption.Return();
        ticket = ticket with
        {
            Properties = ticket.Properties
                .Where(x => !x.Key.EqualsIgnoreCase(addProperty.Key))
                .Append(addProperty)
                .ToImmutableArray()
        };

        return (await Set(ticket, context)).LogStatus(context);
    }

    public async Task<Option> RemoveProperty(string seasonTicketId, string propertyKey, ScopeContext context)
    {
        var test = new OptionTest()
            .Test(() => seasonTicketId.IsNotEmpty(), error: $"{nameof(seasonTicketId)} is required")
            .Test(() => propertyKey.IsEmpty(), error: $"{nameof(propertyKey)} is required");
        if (test.IsError()) return test.Option;

        context = context.With(_logger);
        context.LogInformation("Removing role for seasonTicketId={seasonTicketId}, principalId={principalId}", seasonTicketId, propertyKey);

        var ticketOption = await Get(seasonTicketId, context);
        if (ticketOption.IsError()) return ticketOption.LogStatus(context, "Failed to read").ToOptionStatus();

        var ticket = ticketOption.Return();
        if (ticket.Members.FirstOrDefault(ticket => ticket.PrincipalId.EqualsIgnoreCase(propertyKey)) == null) return StatusCode.NotFound;

        ticket = ticket with
        {
            Properties = ticket.Properties
                .Where(x => !x.Key.EqualsIgnoreCase(propertyKey))
                .ToImmutableArray()
        };

        return (await Set(ticket, context)).LogStatus(context);
    }


    public async Task<Option> AddRole(string seasonTicketId, RoleRecord addRole, ScopeContext context)
    {
        var test = new OptionTest()
            .Test(() => seasonTicketId.IsNotEmpty(), error: $"{nameof(seasonTicketId)} is required")
            .Test(() => addRole.Validate());
        if (test.IsError()) return test.Option;

        context = context.With(_logger);
        context.LogInformation("Adding role for seasonTicketId={seasonTicketId}, principalId={principalId}", seasonTicketId, addRole.PrincipalId);

        var ticketOption = await Get(seasonTicketId, context);
        if (ticketOption.IsError()) return ticketOption.LogStatus(context, "Failed to read").ToOptionStatus();

        var ticket = ticketOption.Return();
        ticket = ticket with
        {
            Members = ticket.Members
                .Where(x => !x.PrincipalId.EqualsIgnoreCase(addRole.PrincipalId))
                .Append(addRole)
                .ToImmutableArray()
        };

        return (await Set(ticket, context)).LogStatus(context);
    }

    public async Task<Option> RemoveRole(string seasonTicketId, string principalId, ScopeContext context)
    {
        var test = new OptionTest()
            .Test(() => seasonTicketId.IsNotEmpty(), error: $"{nameof(seasonTicketId)} is required")
            .Test(() => principalId.IsEmpty(), error: "principalId is required");
        if (test.IsError()) return test.Option;

        context = context.With(_logger);
        context.LogInformation("Removing role for seasonTicketId={seasonTicketId}, principalId={principalId}", seasonTicketId, principalId);

        var ticketOption = await Get(seasonTicketId, context);
        if (ticketOption.IsError()) return ticketOption.LogStatus(context, "Failed to read").ToOptionStatus();

        var ticket = ticketOption.Return();
        if (ticket.Members.FirstOrDefault(ticket => ticket.PrincipalId.EqualsIgnoreCase(principalId)) == null) return StatusCode.NotFound;

        ticket = ticket with
        {
            Members = ticket.Members
                .Where(x => !x.PrincipalId.EqualsIgnoreCase(principalId))
                .ToImmutableArray()
        };

        return (await Set(ticket, context)).LogStatus(context);
    }

    public async Task<Option> AddSeat(string seasonTicketId, SeatRecord seatRecord, ScopeContext context)
    {
        var test = new OptionTest()
            .Test(() => seasonTicketId.IsNotEmpty(), error: $"{nameof(seasonTicketId)} is required")
            .Test(() => seatRecord.Validate());
        if (test.IsError()) return test.Option;

        context.LogInformation(
            "Adding seat, seasonTocketId={seasonTocketId}, seatId={seatId}, eventDate={eventDate}, assignedToPrincipalId={assignedToPrincipalId}",
            seasonTicketId, seatRecord.SeatId, seatRecord.Date, seatRecord.AssignedToPrincipalId
            );

        var ticketOption = await Get(seasonTicketId, context);
        if (ticketOption.IsError()) return ticketOption.LogStatus(context, "Failed to read").ToOptionStatus();

        var ticket = ticketOption.Return();

        ticket = ticket with
        {
            Seats = ticket.Seats
                .Where(x => !(x.SeatId.EqualsIgnoreCase(seatRecord.SeatId) && x.Date == seatRecord.Date))
                .Append(seatRecord)
                .ToImmutableArray()
        };

        return (await Set(ticket, context)).LogStatus(context);
    }

    public async Task<Option> RemoveSeat(string seasonTicketId, string seatId, DateTime eventDate, ScopeContext context)
    {
        var test = new OptionTest()
            .Test(() => seasonTicketId.IsNotEmpty(), error: $"{nameof(seasonTicketId)} is required")
            .Test(() => seatId.IsEmpty(), error: "seatId is required");
        if (test.IsError()) return test.Option;

        context.LogInformation("seat, seasonTocketId={seasonTocketId}, seatId={seatId}, eventDate={eventDate}", seasonTicketId, seatId, eventDate);

        var ticketOption = await Get(seasonTicketId, context);
        if (ticketOption.IsError()) return ticketOption.LogStatus(context, "Failed to read").ToOptionStatus();

        var ticket = ticketOption.Return();
        if (ticket.Seats.FirstOrDefault(x => isMatch(x, seatId, eventDate)) != null) return StatusCode.NotFound;

        ticket = ticket with
        {
            Seats = ticket.Seats
                .Where(x => !isMatch(x, seatId, eventDate))
                .ToImmutableArray()
        };

        return (await Set(ticket, context)).LogStatus(context);

        static bool isMatch(SeatRecord x, string seatId, DateTime eventDate) => x.SeatId.EqualsIgnoreCase(seatId) && x.Date == eventDate;
    }


    public async Task<Option<SeasonTicketRecord>> Get(string seasonTicketId, ScopeContext context)
    {
        if (seasonTicketId.IsEmpty()) return (StatusCode.BadRequest, $"{nameof(seasonTicketId)} is required");
        context = context.With(_logger);
        context.LogInformation("Get seasonTicketId={seasonTicketId}", seasonTicketId);

        string command = SeasonTicketRecord.SelectNodeCommand(seasonTicketId);
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<SeasonTicketRecord>();

        var principalIdentity = resultOption.Return().DataLinks.DataLinkToObject<SeasonTicketRecord>("entity");
        return principalIdentity;
    }

    public async Task<Option> Set(SeasonTicketRecord seasonTicket, ScopeContext context)
    {
        context.With(_logger);
        var test = new OptionTest().Test(() => seasonTicket.Validate());
        if (test.IsError()) return test;

        context.LogInformation("Set seasonTicketId={seasonTicketId}", seasonTicket.SeasonTicketId);

        // Build graph commands 
        string command = SeasonTicketRecord.Schema.Code(seasonTicket).BuildSetCommands().Join(Environment.NewLine);
        var result = await _clusterClient.GetDirectoryActor().ExecuteBatch(command, context);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }
}
