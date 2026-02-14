using System.Text.Json.Serialization;

namespace WaterMeter.Config;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public record WaterMetterConfig
{
    public string TopicId { get; set; }
    public string RefreshToken { get; set; }
    public string CurrentFloor { get; set; } = "1";

    public string MaxFloors { get; set; }
    public int OverWatchInterval { get; set; } = 3;

    public int AdultThreshold { get; set; } = 500;
    public int AlmostAdultThreshold { get; set; } = 0;

    [Newtonsoft.Json.JsonIgnore]
    public string ApiAddress { get; } = "https://api.cc98.org";
}