namespace TicketApi.sdk.Classification.Model;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

public record Root(Embedded _embedded, Links _links, Page page);

public record Classification(bool? family, Links _links, Segment segment, Type type);

public record Embedded(IReadOnlyList<Classification> classifications, IReadOnlyList<Genre> genres, IReadOnlyList<Subgenre> subgenres, IReadOnlyList<Subtype> subtypes);

public record Genre(string id, string name, string locale, Links _links, Embedded _embedded);

public record Links(Self self);

public record Page(int? size, int? totalElements, int? totalPages, int? number);

public record Segment(string id, string name, string locale, string primaryId, Links _links, Embedded _embedded);

public record Self(string href);

public record Subgenre(string id, string name, string locale, Links _links);

public record Subtype(string id, string name, string locale, Links _links);

public record Type(string id, string name, string locale, string primaryId, Links _links, Embedded _embedded);

