using Newtonsoft.Json;

namespace WaterMeter.API;

public class RefreshTokenResopnse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}