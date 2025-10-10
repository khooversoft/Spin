using Toolbox.Extensions;

namespace RaceAlive.sdk;

[Flags]
public enum CourseType
{
    None = 0,
    Flat = 0x10,
    Hilly = 0x20,
    Low = 0x1000,
    Medium = 0x2000,
    High = 0x4000,
};

public class MarathonScheduleModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public DateTime Date { get; set; }
    public bool FeatureRace { get; set; }
    public CourseType CourseType { get; set; }
    public string? PrScore { get; set; }
    public string? BqScore { get; set; }
    public int? Finishers { get; set; }
    public double? PercentQualified { get; set; }
    public string? MarathonSiteUrl { get; set; }
    public IReadOnlyList<Marker> Elevation { get; set; } = [];
    public IReadOnlyList<MarathonReview> Reviews { get; set; } = [];

    public bool IsMatch(string searchTerm)
    {
        bool name = Name?.Like(searchTerm, true) == true;
        bool description = Description?.Like(searchTerm, true) == true;
        bool city = City?.Like(searchTerm, true) == true;
        bool state = State?.Like(searchTerm, true) == true;
        bool date = Date.ToString("yyyy-MM-dd").Like(searchTerm, true) == true;
        bool courseType = CourseType.ToCourseTypeString().Like(searchTerm, true) == true;
        bool prScore = PrScore?.Like(searchTerm, true) == true;
        bool bqScore = BqScore?.Like(searchTerm, true) == true;
        bool finishers = Finishers?.ToString().Like(searchTerm, true) == true;
        bool percentQualified = PercentQualified?.ToString("F1").Like(searchTerm, true) == true;

        return name || description || city || state || date || courseType || prScore || bqScore || finishers || percentQualified;
    }
}

public record Marker
{
    public string Id { get; set; } = null!;
    public int Feet { get; set; }
}

public class MarathonReview
{
    public string Name { get; set; } = null!;
    public string Text { get; set; } = null!;
    public int Rating { get; set; }
    public DateTime SubmittedOn { get; set; } = DateTime.Now;
}

public static class MarathonScheduleModelExtensions
{
    public static string ToCourseTypeString(this CourseType ct) => ct switch
    {
        CourseType.None => "None",

        _ => Enum.GetValues<CourseType>()
            .Where(f => f != CourseType.None && ct.HasFlag(f))
            .Select(f => f.ToString())
            .Join('/')
    };

    public static int GetAverageRating(this MarathonScheduleModel subject)
    {
        if (subject.Reviews == null || !subject.Reviews.Any()) return 0;
        return (int)Math.Round(subject.Reviews.Average(r => r.Rating));
    }
}