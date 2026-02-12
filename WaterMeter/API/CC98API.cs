using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using WaterMeter.Config;
using WaterMeter.Log;
using WaterMeter.Messages;

namespace WaterMeter.API;

public class CC98API
{
    private readonly HttpClient _httpClient;
    public async Task<TopicInfo?> GetTopicInfoAsync(string topicId)
    {
        var result = await _httpClient.GetAsync($"/topic/{topicId}");
        result.EnsureSuccessStatusCode();
        var res = await result.Content.ReadAsStringAsync();
        var topicInfo = JsonConvert.DeserializeObject<TopicInfo>(res);
        new Log.Log(LogLevel.Info, $"刷新主题帖{topicId},标题{topicInfo?.Title},总回复数{topicInfo?.ReplyCount}").Send();
        return topicInfo;
    }

    public async Task<IReadOnlyList<PostInfo>?> GetPostInfoAsync(string topicId, int floor)
    {
        var result = await _httpClient.GetAsync($"/topic/{topicId}/post?from={floor}&size={10}");
        result.EnsureSuccessStatusCode();
        var res = await result.Content.ReadAsStringAsync();
        var postInfos = JsonConvert.DeserializeObject<PostInfo[]>(res);
        new Log.Log(LogLevel.Info, $"获取{floor}到{floor+postInfos?.Length}楼回复用户...成功").Send();
        return postInfos;
    }

    public CC98API(HttpClient httpclient,WaterMetterConfig config)
    {
        httpclient.BaseAddress = new Uri(config.ApiAddress);
        _httpClient = httpclient;
    }

  
}