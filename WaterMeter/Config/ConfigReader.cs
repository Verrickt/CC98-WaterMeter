using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace WaterMeter.Config;

public class ConfigReader
{
    string path = Path.Combine((Environment.CurrentDirectory), "config.json");

    public WaterMetterConfig ReadConfig()
    {
        WaterMetterConfig config = null;
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            config = JsonConvert.DeserializeObject<WaterMetterConfig>(json)!;
        }

        return config ?? new WaterMetterConfig()
            { RefreshToken = "", CurrentFloor = "1", OverWatchInterval = 3, TopicId = "" };
    }

    public async Task SaveConfigAsync(WaterMetterConfig config)
    {
        var json = JsonConvert.SerializeObject(config);
        await File.WriteAllTextAsync(path, json);
    }
}