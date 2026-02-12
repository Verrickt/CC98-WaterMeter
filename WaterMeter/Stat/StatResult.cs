namespace WaterMeter.Stat;

public record StatResult(int TotalReplies, IReadOnlyList<StatEntry> Entries);