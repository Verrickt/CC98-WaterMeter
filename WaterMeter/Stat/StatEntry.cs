namespace WaterMeter.Stat;

public record StatEntry(int UserId, string UserName, int FirstAppearedFloor,DateTimeOffset FirstAppearedTime, int TotalReplyCount)
{
    public DateTimeOffset FirstAppearedTime { get; set; } = FirstAppearedTime;
    public int FirstAppearedFloor { get; set; } = FirstAppearedFloor;
    public int TotalReplyCount { get; set; } = TotalReplyCount;
}