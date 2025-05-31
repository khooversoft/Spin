using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApi.sdk.TicketMasterEvent;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

public record EventRootModel(EmbeddedModel _embedded, LinksModel _links, PageModel page);

public record EmbeddedModel(IReadOnlyList<EventModel> events, IReadOnlyList<VenueModel> venues, IReadOnlyList<AttractionModel> attractions);

public record EventModel(
    string name,
    string type,
    string id,
    bool? test,
    string url,
    string locale,
    IReadOnlyList<ImageModel> images,
    SalesModel sales,
    DatesModel dates,
    IReadOnlyList<ClassificationModel> classifications,
    PromoterModel promoter,
    IReadOnlyList<PromoterModel> promoters,
    SeatmapModel seatmap,
    AccessibilityModel accessibility,
    TicketLimitModel ticketLimit,
    AgeRestrictionsModel ageRestrictions,
    TicketingModel ticketing,
    LinksModel _links,
    EmbeddedModel _embedded,
    IReadOnlyList<ProductModel> products,
    string pleaseNote,
    string info
    );

public record AccessibilityModel(string info, int? ticketLimit, string id, string url, string urlText);

public record AdaModel(string adaPhones, string adaCustomCopy, string adaHours);

public record AddressModel(string line1);

public record AgeRestrictionsModel(bool? legalAgeEnforced, string id);

public record AllInclusivePricingModel(bool? enabled);

public record AttractionModel(
    string href,
    string name,
    string type,
    string id,
    bool? test,
    string url,
    string locale,
    ExternalLinksModel externalLinks,
    IReadOnlyList<ImageModel> images,
    IReadOnlyList<ClassificationModel> classifications,
    UpcomingEventsModel upcomingEvents,
    LinksModel _links,
    IReadOnlyList<string> aliases
    );

public record BoxOfficeInfoModel(string phoneNumberDetail, string openHoursDetail, string acceptedPaymentDetail, string willCallDetail);

public record CityModel(string name);

public record ClassificationModel(bool? primary, SegmentModel segment, GenreModel genre, SubGenreModel subGenre, TypeModel type, SubTypeModel subType, bool? family);

public record CountryModel(string name, string countryCode);

public record DatesModel(StartModel start, string timezone, StatusModel status, bool? spanMultipleDays);

public record DmaModel(int? id);


public record ExternalLinksModel(IReadOnlyList<TwitterModel> twitter, IReadOnlyList<WikiModel> wiki, IReadOnlyList<FacebookModel> facebook, IReadOnlyList<InstagramModel> instagram, IReadOnlyList<HomepageModel> homepage);

public record FacebookModel(string url);

public record FirstModel(string href);

public record GeneralInfoModel(string childRule, string generalRule);

public record GenreModel(string id, string name);

public record HomepageModel(string url);

public record ImageModel(string ratio, string url, int? width, int? height, bool? fallback, string attribution);

public record InstagramModel(string url);

public record LastModel(string href);

public record LinksModel(SelfModel self, IReadOnlyList<AttractionModel> attractions, IReadOnlyList<VenueModel> venues, FirstModel first, NextModel next, LastModel last);

public record LocationModel(string longitude, string latitude);

public record MarketModel(string name, string id);

public record NextModel(string href);

public record PageModel(int? size, int? totalElements, int? totalPages, int? number);

public record PresaleModel(DateTime? startDateTime, DateTime? endDateTime, string name, string description);

public record ProductModel(string name, string id, string url, string type, IReadOnlyList<ClassificationModel> classifications);

public record PromoterModel(string id, string name, string description);

public record Promoter2Model(string id, string name, string description);

public record PublicModel(bool? startTBD, bool? startTBA, DateTime? startDateTime, DateTime? endDateTime);

public record SafeTixModel(bool? enabled);

public record SalesModel(PublicModel @public, IReadOnlyList<PresaleModel> presales);

public record SeatmapModel(string staticUrl, string id);

public record SegmentModel(string id, string name);

public record SelfModel(string href);

public record SocialModel(TwitterModel twitter);

public record StartModel(string localDate, string localTime, DateTime? dateTime, bool? dateTBD, bool? dateTBA, bool? timeTBA, bool? noSpecificTime);

public record StateModel(string name, string stateCode);

public record StatusModel(string code);

public record SubGenreModel(string id, string name);

public record SubTypeModel(string id, string name);

public record TicketingModel(SafeTixModel safeTix, AllInclusivePricingModel allInclusivePricing, string id);

public record TicketLimitModel(string info, string id);

public record TwitterModel(string handle, string url);

public record TypeModel(string id, string name);

public record UpcomingEventsModel(int? ticketmaster, int? _total, int? _filtered, int? archtics, int? tmr);

public record VenueModel(
    string href,
    string name,
    string type,
    string id,
    bool? test,
    string url,
    string locale,
    IReadOnlyList<ImageModel> images,
    string postalCode,
    string timezone,
    CityModel city,
    StateModel state,
    CountryModel country,
    AddressModel address,
    LocationModel location,
    IReadOnlyList<MarketModel> markets,
    IReadOnlyList<DmaModel> dmas,
    BoxOfficeInfoModel boxOfficeInfo,
    string parkingDetail,
    string accessibleSeatingDetail,
    GeneralInfoModel generalInfo,
    UpcomingEventsModel upcomingEvents,
    AdaModel ada,
    LinksModel _links,
    IReadOnlyList<string> aliases,
    SocialModel social
    );

public record WikiModel(string url);

