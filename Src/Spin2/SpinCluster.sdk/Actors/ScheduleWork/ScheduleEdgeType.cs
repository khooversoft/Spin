﻿namespace SpinCluster.sdk.Actors.ScheduleWork;

public enum ScheduleEdgeType
{
    None,
    Active,
    Completed,
    Failed
}


public static class ScheduleEdgeTypeExtensions
{
    private const string _edgeTypePrefix = "scheduleWorkType:";

    public static string GetEdgeType(this ScheduleEdgeType subject) => subject switch
    {
        ScheduleEdgeType.None => throw new ArgumentException("ScheduleEdgeNodeType.None is not valid"),
        var v => $"{_edgeTypePrefix}{v}",
    };
}
