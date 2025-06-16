namespace TicketApi.sdk.TicketMasterAttraction;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

public record AttractionRootModel(Embedded _embedded, Links _links, Page page);

public record Attraction(
    string name,
    string type,
    string id,
    bool? test,
    string url,
    string locale,
    ExternalLinks externalLinks,
    IReadOnlyList<Image> images,
    IReadOnlyList<Classification> classifications,
    UpcomingEvents upcomingEvents,
    Links _links
);

public record Classification(
    bool? primary,
    Segment segment,
    Genre genre,
    SubGenre subGenre,
    Type type,
    SubType subType,
    bool? family
);

public record Embedded(IReadOnlyList<Attraction> attractions);

public record ExternalLinks(
    IReadOnlyList<Twitter> twitter,
    IReadOnlyList<Wiki> wiki,
    IReadOnlyList<Facebook> facebook,
    IReadOnlyList<Instagram> instagram,
    IReadOnlyList<Homepage> homepage
);

public record Facebook(string url);

public record Genre(string id, string name);

public record Homepage(string url);

public record Image(string ratio, string url, int? width, int? height, bool? fallback);

public record Instagram(string url);

public record Links(Self self);

public record Page(int? size, int? totalElements, int? totalPages, int? number);

public record Segment(string id, string name);

public record Self(string href);

public record SubGenre(string id, string name);

public record SubType(string id, string name);

public record Twitter(string url);

public record Type(string id, string name);

public record UpcomingEvents(int? ticketmaster, int? _total, int? _filtered);

public record Wiki(string url);
