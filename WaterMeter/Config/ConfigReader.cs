using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using Newtonsoft.Json;

namespace WaterMeter.Config;

public class ConfigReader
{

    public string BasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WaterMaker");
    private readonly string _configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WaterMaker",
        "config.json"
    );

    public WaterMetterConfig ReadConfig()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                using var reader = File.OpenText(_configPath);
                using var jsonReader = new JsonTextReader(reader);
                var serializer = new JsonSerializer();
                return serializer.Deserialize<WaterMetterConfig>(jsonReader)
                       ?? CreateDefaultConfig();
            }
            catch
            {
                // If file is corrupted, fall back to default
                return CreateDefaultConfig();
            }
        }

        return CreateDefaultConfig();
    }

    public async Task SaveConfigAsync(WaterMetterConfig config)
    {
        // Ensure the directory exists (in case it was deleted)
        var directory = Path.GetDirectoryName(_configPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        // Use Create mode to overwrite existing config
        await using var fs = new FileStream(_configPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await using var writer = new StreamWriter(fs);
        var serializer = new JsonSerializer();
        serializer.Serialize(writer, config);
    }

    private WaterMetterConfig CreateDefaultConfig() => new WaterMetterConfig
    {
        RefreshToken = "",
        CurrentFloor = "1",
        OverWatchInterval = 3,
        TopicId = ""
    };
}