using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace TicketApi.sdk;

public class IconCollectionService
{
    private readonly TicketOption _ticketOption;
    private readonly ILogger<IconCollectionService> _logger;
    private IDictionary<string, string>? _map;
    private readonly object _lock = new object();
    private readonly TicketMasterClient _ticketMasterClient;
    private readonly FrozenSet<string> _validRatios = FrozenSet.Create<string>(StringComparer.OrdinalIgnoreCase, "4_3", "3_4");

    private static readonly IDictionary<string, string> _baseMap = new Dictionary<string, string>
    {
        { "music", "select/musical-note-50.png" },
        { "sports", "select/trophy-32.png" },
        { "football", "select/football.png" },
        { "basketball", "select/basketball-50.png" },
        { "baseball", "select/baseball-48.png" },
        { "hockey", "select/hockey-50.png" },
        { "soccer", "select/soccer-50.png" }
    };

    public IconCollectionService(TicketOption ticketOption, TicketMasterClient ticketMasterClient, ILogger<IconCollectionService> logger)
    {
        _ticketOption = ticketOption.NotNull();
        _ticketMasterClient = ticketMasterClient.NotNull();
        _logger = logger.NotNull();
    }

    public void AddAndMerge(AttractionCollectionRecord attractionCollection, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Adding and merging icons for {count} attractions", attractionCollection.Attractions.Count);

        var attractions = attractionCollection.Attractions
            .Select(x => (attraction: x, image: findUsableImage(x)))
            .Where(x => x.image != null)
            .Select(x => new KeyValuePair<string, string>(x.attraction.Name, x.image!.Url))
            .ToArray();

        lock (_lock)
        {
            var baseMap = _map ?? BuildStatic();
            var merged = Merge(baseMap, attractions);

            _map = merged;
        }

        return;


        ImageRecord? findUsableImage(AttractionRecord attraction)
        {
            ImageRecord? result = attraction.Images
                .Where(x => x.Width != null && x.Height != null && _validRatios.Contains(x.Ratio))
                .Select(x => (image: x, dist: distance(x)))
                .GroupBy(x => x.dist)
                .Select(x => x.First().image)
                .FirstOrDefault();

            return result;
        }

        int distance(ImageRecord image)
        {
            if (image.Width == null || image.Height == null) return int.MinValue;
            return Math.Abs(image.Width.Value - image.Height.Value);
        }
    }

    public bool TryGetValue(string key, [MaybeNullWhen(returnValue: false)] out string value)
    {
        lock (_lock)
        {
            _map ??= BuildStatic();
        }

        return _map.TryGetValue(key, out value);
    }

    private IDictionary<string, string> BuildStatic()
    {
        _logger.LogDebug("Building icon map for TicketOption");

        var list = new Sequence<KeyValuePair<string, string>>();

        list += _ticketOption.Logos.Select(x => new KeyValuePair<string, string>(x.MatchTo, x.LogoUrl));

        foreach (var include in _ticketOption.IncludeLogoFiles)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(include)
                .Build();

            var configMap = configuration.Get<LogoCollection>("Ticket");
            list += configMap.Logos.Select(x => new KeyValuePair<string, string>(x.MatchTo, x.LogoUrl));
        }

        var newMap = Merge(_baseMap, list);

        return newMap;
    }

    private IDictionary<string, string> Merge(IDictionary<string, string> baseSource, IEnumerable<KeyValuePair<string, string>> newSource)
    {
        var newMap = baseSource
            .Select(x => (level: 0, data: x))
            .Concat(newSource.Select(x => (level: 1, data: x)))
            .GroupBy(x => x.data.Key)
            .Select(x => x.OrderBy(y => y.level).First().data)
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        return newMap;
    }
}
