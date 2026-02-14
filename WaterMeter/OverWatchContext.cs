using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WaterMeter.API;
using WaterMeter.Config;
using WaterMeter.Messages;
using WaterMeter.SimpleLog;

namespace WaterMeter;

public class OverWatchContext : IRecipient<ConfigChangedMessage>,IRecipient<OverwatcherRequestMessage>
{
    public TopicInfo? CachedTopicInfo { get; private set; }
    public int OverwatchInterval => config.OverWatchInterval;
    public string TopicId => config.TopicId;
    public int CurrentFloor { get; private set; }
    public int MaxFloor => int.Parse(config.MaxFloors);

    [NotNull]
    private HashSet<int> _cachedFloors = [];
    private readonly WaterMetterConfig config;
    private readonly ConfigReader reader;
    private readonly CacheManager cacheManager;

    public OverWatchContext(WaterMetterConfig config, ConfigReader reader, CacheManager cacheManager)
    {
        this.config = config;
        this.reader = reader;
        this.cacheManager = cacheManager;
        WeakReferenceMessenger.Default.Register<ConfigChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<OverwatcherRequestMessage>(this);
    }

    public bool IsFloorCached(int floor)
    {
        return _cachedFloors.Contains(floor);
    }

    public int? ToNextUncachedFloor()
    {
        int current = CurrentFloor;
        var floors = _cachedFloors.ToHashSet();
        var uncached = current;
        while (true)
        {
            if (!floors.Contains(current)) 
            {
                uncached = current;
                break;
            }
            current++;
        }
        return uncached;
    }
    public async Task InitAsync()
    {
        new Log(LogLevel.Info, "缓存重建中...").Send();
        CurrentFloor = int.Parse(config.CurrentFloor);
        CachedTopicInfo = await cacheManager.ReadTopicInfoAsync(config.TopicId);
        _cachedFloors =
            [.. await cacheManager.ReadRepliesAsync(config.TopicId).Select(i => i.Floor).ToListAsync()];
        new Log(LogLevel.Info, "缓存重建完成").Send();
    }
    public async Task UpdateTopicInfoAsync(TopicInfo? topicInfo)
    {
        if (topicInfo != null)
        {
            CachedTopicInfo = topicInfo;
            await cacheManager.UpdateTopicInfo(topicInfo);
            config.MaxFloors = (topicInfo.ReplyCount + 1).ToString();
            WeakReferenceMessenger.Default.Send(new ConfigChangedMessage(config));
        }
    }

    public async Task UpdateReplyInfoAsync(IEnumerable<PostInfo>? postInfos)
    {
        var filtered = postInfos?.Where(i => !_cachedFloors.Contains(i.Floor))?.ToList();
        if (filtered?.Count > 0)
        {
            await cacheManager.AppendReplyInfos(TopicId, filtered);
            filtered.ForEach(f => _cachedFloors.Add(f.Floor));
        }
    }
    public async Task AdvanceToEnd()
    {
        await AdvanceAsync(MaxFloor-CurrentFloor+1);
    }
    public async Task AdvanceAsync(int? count)
    {
        CurrentFloor += count ?? 0;
        config.CurrentFloor = CurrentFloor.ToString();
        WeakReferenceMessenger.Default.Send(new ConfigChangedMessage(config));
        await reader.SaveConfigAsync(config);
    }

    public async void Receive(ConfigChangedMessage message)
    {
        if (message.Value.TopicId != CachedTopicInfo?.Id.ToString())
        {
            CachedTopicInfo = null;
            await InitAsync();
        }
        this.CurrentFloor = int.Parse(message.Value.CurrentFloor);
    }

    public async void Receive(OverwatcherRequestMessage message)
    {
        if (message.Action == OverwatcherAction.RebuildCache)
        {
            new Log(LogLevel.Info, "缓存重建中...").Send();
            CachedTopicInfo = await cacheManager.ReadTopicInfoAsync(config.TopicId);
            _cachedFloors =
                [.. await cacheManager.ReadRepliesAsync(config.TopicId).Select(i => i.Floor).ToListAsync()];
            CurrentFloor = 1;
            new Log(LogLevel.Info, "缓存重建完成").Send();
        }
    }
}