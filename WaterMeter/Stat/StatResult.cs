namespace WaterMeter.Stat;

public record StatResult(int TotalReplies,int DeletedReplies,bool HasGap, IReadOnlyList<StatEntry> Adults
    , IReadOnlyList<StatEntry> AlmostAdults);