using RaceAlive.sdk.TestData;
using Toolbox.Extensions;
using Toolbox.Types;

namespace RaceAlive.sdk;

public class MarathonScheduleClient
{
    public MarathonScheduleClient()
    {
    }

    public Task<Option<IReadOnlyList<MarathonScheduleModel>>> GetMarathonSchedule()
    {
        return MarathonScheduleTestData.Marathons.ToOption().ToTaskResult();
    }

    public async Task<Option<MarathonScheduleModel>> Get(string id) => (await GetMarathonSchedule())
        .ThrowOnError().Return()
        .FirstOrDefaultOption(x => x.Id == id, returnNotFound: true);
}
