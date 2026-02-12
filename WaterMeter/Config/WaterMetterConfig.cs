using System.Text.Json.Serialization;

namespace WaterMeter.Config;

public record WaterMetterConfig
{
    public string TopicId { get; set; }
    public string RefreshToken { get; set; }
    public string CurrentFloor { get; set; } = "1";

    public string MaxFloors { get; set; }
    public int OverWatchInterval { get; set; } = 3;

    [Newtonsoft.Json.JsonIgnore]
    public string ApiAddress { get; } = "https://api.cc98.org";
}