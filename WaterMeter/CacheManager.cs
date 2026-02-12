using Newtonsoft.Json;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Runtime.CompilerServices;
using WaterMeter.API;

namespace WaterMeter;

public class CacheManager
{
    // 获取 %AppData%/WaterMaker 路径
    private readonly string _basePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WaterMaker"
    );

    public string BasePath => _basePath;
    public CacheManager()
    {
        // 确保文件夹存在
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    private string GetFilePath(string fileName) => Path.Combine(_basePath, fileName);

    public async Task<TopicInfo?> ReadTopicInfoAsync(string topicId, CancellationToken token = default)
    {
        var filePath = GetFilePath($"{topicId}-topic.json");
        if (!File.Exists(filePath)) return null;

        using var reader = File.OpenText(filePath);
        var jsonTextReader = new JsonTextReader(reader);
        var serializer = new JsonSerializer();
        return serializer.Deserialize<TopicInfo>(jsonTextReader);
    }

    public async IAsyncEnumerable<PostInfo> ReadReplysAsync(string topicId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var filePath = GetFilePath($"{topicId}-replies.json");
        if (!File.Exists(filePath)) yield break;

        using var reader = new StreamReader(filePath);
        while (!token.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(token);
            if (line == null) break;

            var posts = JsonConvert.DeserializeObject<IEnumerable<PostInfo>>(line) ?? [];
            foreach (var post in posts)
            {
                yield return post;
            }
        }
    }

    public async Task UpdateTopicInfo(TopicInfo topicInfo, CancellationToken cancellation = default)
    {
        var filePath = GetFilePath($"{topicInfo.Id}-topic.json");

        // 使用 FileStream 确保异步写入
        await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await using var writer = new StreamWriter(file);
        var jsonTextWriter = new JsonTextWriter(writer);
        var serializer = new JsonSerializer();
        serializer.Serialize(jsonTextWriter, topicInfo);
    }

    public async Task AppendReplyInfos(string topicId, IEnumerable<PostInfo> postInfos, CancellationToken cancellation = default)
    {
        var filePath = GetFilePath($"{topicId}-replies.json");

        // 使用 Append 模式
        await using var file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, true);
        await using var writer = new StreamWriter(file);
        var jsonTextWriter = new JsonTextWriter(writer);
        var serializer = new JsonSerializer();
        await jsonTextWriter.WriteRawAsync(Environment.NewLine, cancellation);
        serializer.Serialize(jsonTextWriter, postInfos);
    }
}