using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace RaceAlive.sdk.TestData;

public static class MarathonScheduleTestData
{
    public static IReadOnlyList<MarathonScheduleModel> Marathons { get; } = new List<MarathonScheduleModel>()
    {
        new MarathonScheduleModel
        {
            Name = "Boston Marathon",
            Description = "Historic point-to-point course with late Newton hills; net downhill early.",
            City = "Boston",
            State = "MA",
            Date = GetDate(),
            FeatureRace = true,
            CourseType = CourseType.Hilly | CourseType.Medium,
            PrScore = "9:30",
            BqScore = "85.50%",
            Finishers = 30000,
            PercentQualified = 0.10,
            MarathonSiteUri = "https://www.baa.org/races/boston-marathon",
            ElevationChartUri = "/elevationChart/boston",
            ReviewsUri = "/review/boston",
            Elevation = GetElevation(CourseType.Hilly | CourseType.Medium),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Chicago Marathon",
            Description = "Flat and fast urban loop through diverse neighborhoods.",
            City = "Chicago",
            State = "IL",
            Date = GetDate(),
            FeatureRace = true,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:45",
            BqScore = "78.25%",
            Finishers = 40000,
            PercentQualified = 0.14,
            MarathonSiteUri = "https://www.chicagomarathon.com",
            ElevationChartUri = "/elevationChart/chicago",
            ReviewsUri = "/review/chicago",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "New York City Marathon",
            Description = "Five-borough tour; bridges and rolling Central Park finish.",
            City = "New York",
            State = "NY",
            Date = GetDate(),
            FeatureRace = true,
            CourseType = CourseType.Hilly | CourseType.Medium,
            PrScore = "9:05",
            BqScore = "88.75%",
            Finishers = 50000,
            PercentQualified = 0.09,
            MarathonSiteUri = "https://www.nyrr.org/tcsnycmarathon",
            ElevationChartUri = "/elevationChart/nyc",
            ReviewsUri = "/review/nyc",
            Elevation = GetElevation(CourseType.Hilly | CourseType.Medium),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "London Marathon",
            Description = "Fast point-to-point style with iconic city landmarks.",
            City = "London",
            State = "UK",
            Date = GetDate(),
            FeatureRace = true,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:50",
            BqScore = "72.30%",
            Finishers = 42000,
            PercentQualified = 0.16,
            MarathonSiteUri = "https://www.tcslondonmarathon.com",
            ElevationChartUri = "/elevationChart/london",
            ReviewsUri = "/review/london",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Berlin Marathon",
            Description = "Widely considered the fastest world marathon; record-friendly flat streets.",
            City = "Berlin",
            State = "DE",
            Date = GetDate(),
            FeatureRace = true,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:55",
            BqScore = "65.80%",
            Finishers = 45000,
            PercentQualified = 0.20,
            MarathonSiteUri = "https://www.bmw-berlin-marathon.com",
            ElevationChartUri = "/elevationChart/berlin",
            ReviewsUri = "/review/berlin",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Tokyo Marathon",
            Description = "Modern city course, cool weather, strong elite field.",
            City = "Tokyo",
            State = "JP",
            Date = GetDate(),
            FeatureRace = true,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:40",
            BqScore = "74.15%",
            Finishers = 37000,
            PercentQualified = 0.15,
            MarathonSiteUri = "https://www.marathon.tokyo/en/",
            ElevationChartUri = "/elevationChart/tokyo",
            ReviewsUri = "/review/tokyo",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Paris Marathon",
            Description = "Scenic urban tour; moderate crowding early, largely flat.",
            City = "Paris",
            State = "FR",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:35",
            BqScore = "81.40%",
            Finishers = 52000,
            PercentQualified = 0.12,
            MarathonSiteUri = "https://www.parismarathon.com",
            ElevationChartUri = "/elevationChart/paris",
            ReviewsUri = "/review/paris",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Los Angeles Marathon",
            Description = "Point-to-point with rolling sections; variable coastal weather.",
            City = "Los Angeles",
            State = "CA",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Hilly | CourseType.Medium,
            PrScore = "8:55",
            BqScore = "89.95%",
            Finishers = 22000,
            PercentQualified = 0.07,
            MarathonSiteUri = "https://www.lamarathon.com",
            ElevationChartUri = "/elevationChart/losangeles",
            ReviewsUri = "/review/losangeles",
            Elevation = GetElevation(CourseType.Hilly | CourseType.Medium),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Houston Marathon",
            Description = "Cool January weather and flat streets ideal for PR attempts.",
            City = "Houston",
            State = "TX",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:30",
            BqScore = "69.45%",
            Finishers = 12000,
            PercentQualified = 0.18,
            MarathonSiteUri = "https://www.chevronhoustonmarathon.com",
            ElevationChartUri = "/elevationChart/houston",
            ReviewsUri = "/review/houston",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Grandma's Marathon",
            Description = "Net downhill point-to-point along Lake Superior; cool early summer temps.",
            City = "Duluth",
            State = "MN",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:45",
            BqScore = "67.60%",
            Finishers = 8000,
            PercentQualified = 0.19,
            MarathonSiteUri = "https://grandmasmarathon.com",
            ElevationChartUri = "/elevationChart/grandmas",
            ReviewsUri = "/review/grandmas",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Twin Cities Marathon",
            Description = "Scenic urban/park lakes course; gentle rolling hills late.",
            City = "Minneapolis",
            State = "MN",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Hilly | CourseType.Medium,
            PrScore = "9:05",
            BqScore = "79.25%",
            Finishers = 7500,
            PercentQualified = 0.13,
            MarathonSiteUri = "https://www.tcmevents.org/events/medtronic-twin-cities-marathon-weekend",
            ElevationChartUri = "/elevationChart/twincities",
            ReviewsUri = "/review/twincities",
            Elevation = GetElevation(CourseType.Hilly | CourseType.Medium),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Marine Corps Marathon",
            Description = "Scenic Washington D.C. and Arlington; rolling with early climbs.",
            City = "Arlington",
            State = "VA",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Hilly | CourseType.Medium,
            PrScore = "8:40",
            BqScore = "87.20%",
            Finishers = 19000,
            PercentQualified = 0.08,
            MarathonSiteUri = "https://www.marinemarathon.com",
            ElevationChartUri = "/elevationChart/marinecorps",
            ReviewsUri = "/review/marinecorps",
            Elevation = GetElevation(CourseType.Hilly | CourseType.Medium),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Philadelphia Marathon",
            Description = "Fast fall race; mixed urban and river out-and-backs.",
            City = "Philadelphia",
            State = "PA",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Medium,
            PrScore = "9:20",
            BqScore = "74.15%",
            Finishers = 11000,
            PercentQualified = 0.15,
            MarathonSiteUri = "https://www.philadelphiamarathon.com",
            ElevationChartUri = "/elevationChart/philadelphia",
            ReviewsUri = "/review/philadelphia",
            Elevation = GetElevation(CourseType.Flat | CourseType.Medium),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Honolulu Marathon",
            Description = "Warm, humid tropical conditions; scenic coastal and volcanic vistas.",
            City = "Honolulu",
            State = "HI",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Hilly | CourseType.Medium,
            PrScore = "7:50",
            BqScore = "90.00%",
            Finishers = 20000,
            PercentQualified = 0.04,
            MarathonSiteUri = "https://www.honolulumarathon.org",
            ElevationChartUri = "/elevationChart/honolulu",
            ReviewsUri = "/review/honolulu",
            Elevation = GetElevation(CourseType.Hilly | CourseType.Medium),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "California International Marathon",
            Description = "Net downhill from Folsom to Sacramento; cool December temps.",
            City = "Sacramento",
            State = "CA",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:50",
            BqScore = "62.35%",
            Finishers = 9000,
            PercentQualified = 0.23,
            MarathonSiteUri = "https://www.runcim.org",
            ElevationChartUri = "/elevationChart/cim",
            ReviewsUri = "/review/cim",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Valencia Marathon",
            Description = "Very flat, fast European winter marathon; modern course design.",
            City = "Valencia",
            State = "ES",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:55",
            BqScore = "64.75%",
            Finishers = 25000,
            PercentQualified = 0.21,
            MarathonSiteUri = "https://www.valenciaciudaddelrunning.com/en/events/marathon/",
            ElevationChartUri = "/elevationChart/valencia",
            ReviewsUri = "/review/valencia",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Amsterdam Marathon",
            Description = "Fast autumn race; flat city and park segments.",
            City = "Amsterdam",
            State = "NL",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:40",
            BqScore = "69.45%",
            Finishers = 17000,
            PercentQualified = 0.18,
            MarathonSiteUri = "https://www.tcsamsterdammarathon.nl/en/",
            ElevationChartUri = "/elevationChart/amsterdam",
            ReviewsUri = "/review/amsterdam",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Rotterdam Marathon",
            Description = "Reputed flat spring European course ideal for PRs.",
            City = "Rotterdam",
            State = "NL",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "9:50",
            BqScore = "65.80%",
            Finishers = 14500,
            PercentQualified = 0.20,
            MarathonSiteUri = "https://www.nnmarathonrotterdam.org/en/",
            ElevationChartUri = "/elevationChart/rotterdam",
            ReviewsUri = "/review/rotterdam",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Austin Marathon",
            Description = "Challenging rolling course; warmer early-season conditions.",
            City = "Austin",
            State = "TX",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Hilly | CourseType.High,
            PrScore = "7:30",
            BqScore = "89.05%",
            Finishers = 6000,
            PercentQualified = 0.05,
            MarathonSiteUri = "https://www.youraustinmarathon.com",
            ElevationChartUri = "/elevationChart/austin",
            ReviewsUri = "/review/austin",
            Elevation = GetElevation(CourseType.Hilly | CourseType.High),
            Reviews = CreateReviews(),
        },
        new MarathonScheduleModel
        {
            Name = "Miami Marathon",
            Description = "Scenic coastal winter marathon; warm and humid.",
            City = "Miami",
            State = "FL",
            Date = GetDate(),
            FeatureRace = false,
            CourseType = CourseType.Flat | CourseType.Low,
            PrScore = "8:20",
            BqScore = "88.10%",
            Finishers = 6000,
            PercentQualified = 0.06,
            MarathonSiteUri = "https://www.themiamimarathon.com",
            ElevationChartUri = "/elevationChart/miami",
            ReviewsUri = "/review/miami",
            Elevation = GetElevation(CourseType.Flat | CourseType.Low),
            Reviews = CreateReviews(),
        },
    };

    // --- Date helpers (approximate heuristics) ---

    private static DateTime GetDate() => DateTime.Now.AddDays(RandomNumberGenerator.GetInt32(10, 200));

    private static IReadOnlyList<MarathonReview> CreateReviews() => Enumerable.Range(0, RandomNumberGenerator.GetInt32(1, 10))
        .Select(x => new MarathonReview
        {
            Name = $"User{RandomNumberGenerator.GetInt32(1000, 9999)}",
            Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            Rating = RandomNumberGenerator.GetInt32(1, 5),
            SubmittedOn = DateTime.Now.AddDays(-RandomNumberGenerator.GetInt32(1, 365))
        })
        .ToArray();

    private static IReadOnlyList<Marker> GetElevation(CourseType courseType)
    {
        // Generate 27 points: Start + 25 mid markers (M1..M25) + Finish
        const int midMarkers = 25;
        int points = midMarkers + 2;

        // Create raw deltas with gentle trending to look like real hills
        var deltas = new double[points - 1];
        bool up = RandomNumberGenerator.GetInt32(0, 2) == 0;
        int streak = 0;

        for (int i = 0; i < deltas.Length; i++)
        {
            // Flip trend occasionally to form hills (longer streak lowers flip odds)
            if (RandomNumberGenerator.GetInt32(0, 100) < Math.Max(10, 30 - streak))
            {
                up = !up;
                streak = 0;
            }
            else
            {
                streak++;
            }

            // Base magnitude before scaling: 5..25 feet per segment
            double magnitude = 5 + NextDouble() * 20;
            deltas[i] = (up ? 1 : -1) * magnitude;
        }

        // Compute scaling based on course type
        if (courseType.HasFlag(CourseType.Flat))
        {
            // Clamp overall elevation range (max-min) to <= 100 ft
            var raw = Integrate(deltas, 0);
            double range = raw.Max() - raw.Min();
            double scale = range > 100 ? (100.0 / range) : 1.0;
            Scale(deltas, scale);
        }
        else if (courseType.HasFlag(CourseType.Hilly))
        {
            // Ensure total elevation gain (sum of positive deltas) in 600..1200 ft
            // Bias target by difficulty flags if present
            double targetGain =
                courseType.HasFlag(CourseType.High) ? 1100 :
                courseType.HasFlag(CourseType.Medium) ? 900 :
                courseType.HasFlag(CourseType.Low) ? 700 : 900;

            double rawGain = deltas.Where(d => d > 0).Sum();
            if (rawGain < 1) rawGain = 1; // avoid divide by zero
            double scale = targetGain / rawGain;
            Scale(deltas, scale);

            // As a safety, cap to 600..1200 total gain
            double finalGain = deltas.Where(d => d > 0).Sum();
            if (finalGain < 600) Scale(deltas, 600.0 / Math.Max(1, finalGain));
            else if (finalGain > 1200) Scale(deltas, 1200.0 / finalGain);
        }
        else
        {
            // Default gentle course: keep as-is (acts like low undulation)
        }

        // Integrate to absolute elevation values. Start near sea level but keep positive.
        var elevations = Integrate(deltas, 100);
        double min = elevations.Min();
        if (min < 0)
        {
            double offset = -min + 10;
            for (int i = 0; i < elevations.Count; i++) elevations[i] += offset;
        }

        // Convert to markers (S, M1..M25, F)
        var list = new List<Marker>(points)
        {
            new Marker { Id = "S", Feet = RoundFeet(elevations[0]) }
        };

        for (int i = 1; i <= midMarkers; i++)
        {
            list.Add(new Marker { Id = $"M{i}", Feet = RoundFeet(elevations[i]) });
        }

        list.Add(new Marker { Id = "F", Feet = RoundFeet(elevations[^1]) });
        return list;

        // Helpers
        static double NextDouble() =>
            RandomNumberGenerator.GetInt32(0, int.MaxValue) / (double)int.MaxValue;

        static void Scale(double[] values, double scale)
        {
            for (int i = 0; i < values.Length; i++) values[i] *= scale;
        }

        static List<double> Integrate(double[] deltas, double start)
        {
            var result = new List<double>(deltas.Length + 1) { start };
            double current = start;
            for (int i = 0; i < deltas.Length; i++)
            {
                current += deltas[i];
                result.Add(current);
            }
            return result;
        }

        static int RoundFeet(double v) => (int)Math.Round(v, MidpointRounding.AwayFromZero);
    }
}
