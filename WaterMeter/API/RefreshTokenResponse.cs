using Newtonsoft.Json;

namespace WaterMeter.API;

public class RefreshTokenResponse
{
    [JsonProperty("access_token")]
    public required string AccessToken { get; set; }
}