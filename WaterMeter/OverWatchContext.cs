using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WaterMeter.API;
using WaterMeter.Config;
using WaterMeter.Messages;

namespace WaterMeter;

public class OverWatchContext(WaterMetterConfig config,ConfigReader reader):IRecipient<ConfigChangedMessage>
{
    public TopicInfo CachedTopicInfo { get; private set; }
    public int OverwatchInterval => config.OverWatchInterval;
    public string TopicId => config.TopicId;
    public int CurrentFloor { get; private set; }
    public int MaxFloor => int.Parse(config.MaxFloors);
    private DirectoryInfo CacheDir => new(Path.Combine(Environment.CurrentDirectory,"data"));
    internal string TopicCachePath => Path.Join(CacheDir.FullName, $"{TopicId}-topic.json");
    internal string ReplyCachePath => Path.Join(CacheDir.FullName, $"{TopicId}-replies.json");


    [NotNull]
    private HashSet<int> _cachedFloors = new HashSet<int>();
    public void Init()
    {
        CacheDir.Create();
        _cachedFloors = new HashSet<int>();
        CurrentFloor = int.Parse(config.CurrentFloor);
        if (File.Exists(TopicCachePath))
        {
            var json = File.ReadAllText(TopicCachePath);
            CachedTopicInfo = JsonConvert.DeserializeObject<TopicInfo>(json)!;
        }
        if (File.Exists(ReplyCachePath))
        {
            var lines = File.ReadAllLines(ReplyCachePath);
            foreach (var line in lines)
            {
                JsonConvert.DeserializeObject<List<PostInfo>>(line)?
                    .ForEach(r=>_cachedFloors.Add(r.Floor));
            }
        }
        WeakReferenceMessenger.Default.Register(this);
    }
    public async Task UpdateTopicInfoAsync(TopicInfo? topicInfo)
    {
        if (topicInfo != null)
        {
            CachedTopicInfo = topicInfo;
            var json = JsonConvert.SerializeObject(topicInfo);
            await File.WriteAllTextAsync(TopicCachePath, json);
            config.MaxFloors = (topicInfo.ReplyCount+1).ToString();
            WeakReferenceMessenger.Default.Send(new ConfigChangedMessage(config));
        }
    }

    public async Task UpdateReplyInfoAsync(IEnumerable<PostInfo>? postInfos)
    {
        var filtered = postInfos?.Where(i => !_cachedFloors.Contains(i.Floor))?.ToList();
        if (filtered?.Count>0)
        {
            var json = JsonConvert.SerializeObject(postInfos);
            if (File.Exists(ReplyCachePath))
            {
                await File.AppendAllTextAsync(ReplyCachePath,Environment.NewLine);
            }
            await File.AppendAllTextAsync(ReplyCachePath, json);
            filtered.ForEach(f=>_cachedFloors.Add(f.Floor));
        }
    }

    public async Task AdvanceAsync(int? count)
    {
        CurrentFloor += count??0;
        config.CurrentFloor = CurrentFloor.ToString();
        WeakReferenceMessenger.Default.Send(new ConfigChangedMessage(config));
        await reader.SaveConfigAsync(config);
    }

    public void Receive(ConfigChangedMessage message)
    {
        if (message.Value.TopicId != CachedTopicInfo?.Id.ToString())
        {
            CachedTopicInfo = null;
            Init();
        }
        this.CurrentFloor = int.Parse(message.Value.CurrentFloor);
    }
}