using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WaterMeter.API;
using WaterMeter.Config;
using WaterMeter.Messages;

namespace WaterMeter;

public class OverWatchContext(WaterMetterConfig config,ConfigReader reader, CacheManager cacheManager) :IRecipient<ConfigChangedMessage>
{
    public TopicInfo? CachedTopicInfo { get; private set; }
    public int OverwatchInterval => config.OverWatchInterval;
    public string TopicId => config.TopicId;
    public int CurrentFloor { get; private set; }
    public int MaxFloor => int.Parse(config.MaxFloors);

    [NotNull]
    private HashSet<int> _cachedFloors = new();
    public async Task InitAsync()
    {

        CurrentFloor = int.Parse(config.CurrentFloor);
        CachedTopicInfo = await cacheManager.ReadTopicInfoAsync(config.TopicId);
        _cachedFloors =
            new HashSet<int>(await cacheManager.ReadReplysAsync(config.TopicId).Select(i => i.Floor).ToListAsync());
            
        WeakReferenceMessenger.Default.Register(this);
    }
    public async Task UpdateTopicInfoAsync(TopicInfo? topicInfo)
    {
        if (topicInfo != null)
        {
            CachedTopicInfo = topicInfo;
            await cacheManager.UpdateTopicInfo(topicInfo);
            config.MaxFloors = (topicInfo.ReplyCount+1).ToString();
            WeakReferenceMessenger.Default.Send(new ConfigChangedMessage(config));
        }
    }

    public async Task UpdateReplyInfoAsync(IEnumerable<PostInfo>? postInfos)
    {
        var filtered = postInfos?.Where(i => !_cachedFloors.Contains(i.Floor))?.ToList();
        if (filtered?.Count>0)
        {
            await cacheManager.AppendReplyInfos(TopicId,filtered);
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

    public async void Receive(ConfigChangedMessage message)
    {
        if (message.Value.TopicId != CachedTopicInfo?.Id.ToString())
        {
            CachedTopicInfo = null;
            await InitAsync();
        }
        this.CurrentFloor = int.Parse(message.Value.CurrentFloor);
    }
}