using Newtonsoft.Json;

namespace WaterMeter.API;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public class TopicInfo
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("boardId")]
    public int BoardId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("time")]
    public DateTimeOffset Time { get; set; }

    [JsonProperty("userId")]
    public int UserId { get; set; }

    [JsonProperty("userName")]
    public string UserName { get; set; }

    [JsonProperty("userInfo")]
    public object UserInfo { get; set; }

    [JsonProperty("isAnonymous")]
    public bool IsAnonymous { get; set; }

    [JsonProperty("disableHot")]
    public bool DisableHot { get; set; }

    [JsonProperty("lastPostTime")]
    public DateTimeOffset LastPostTime { get; set; }

    [JsonProperty("state")]
    public int State { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("replyCount")]
    public int ReplyCount { get; set; }

    [JsonProperty("hitCount")]
    public int HitCount { get; set; }

    [JsonProperty("totalVoteUserCount")]
    public int TotalVoteUserCount { get; set; }

    [JsonProperty("lastPostUser")]
    public string LastPostUser { get; set; }

    [JsonProperty("lastPostContent")]
    public string LastPostContent { get; set; }

    [JsonProperty("topState")]
    public int TopState { get; set; }

    [JsonProperty("bestState")]
    public int BestState { get; set; }

    [JsonProperty("isVote")]
    public bool IsVote { get; set; }

    [JsonProperty("isPosterOnly")]
    public bool IsPosterOnly { get; set; }

    [JsonProperty("allowedViewerState")]
    public int AllowedViewerState { get; set; }

    [JsonProperty("dislikeCount")]
    public int DislikeCount { get; set; }

    [JsonProperty("likeCount")]
    public int LikeCount { get; set; }

    [JsonProperty("highlightInfo")]
    public object HighlightInfo { get; set; }

    [JsonProperty("tag1")]
    public int Tag1 { get; set; }

    [JsonProperty("tag2")]
    public int Tag2 { get; set; }

    [JsonProperty("isInternalOnly")]
    public bool IsInternalOnly { get; set; }

    [JsonProperty("notifyPoster")]
    public bool NotifyPoster { get; set; }

    [JsonProperty("isMe")]
    public bool IsMe { get; set; }

    [JsonProperty("todayCount")]
    public int TodayCount { get; set; }

    [JsonProperty("allowHotReply")]
    public bool AllowHotReply { get; set; }

    [JsonProperty("contentType")]
    public int ContentType { get; set; }

    [JsonProperty("favoriteCount")]
    public int FavoriteCount { get; set; }

    [JsonProperty("topicAuthorPermissions")]
    public List<object> TopicAuthorPermissions { get; set; }


    [JsonProperty("specialStyle")]
    public int SpecialStyle { get; set; }

    [JsonProperty("notifyAllReplierCountByLZ")]
    public int NotifyAllReplierCountByLZ { get; set; }

    [JsonProperty("lastNotifyAllReplierFloorByLZ")]
    public int LastNotifyAllReplierFloorByLZ { get; set; }

    [JsonProperty("canNotifyAllReplier")]
    public bool CanNotifyAllReplier { get; set; }

    [JsonProperty("notifyAllReplierPostIds")]
    public List<int> NotifyAllReplierPostIds { get; set; }

    [JsonProperty("isHotTopic")]
    public bool IsHotTopic { get; set; }

    [JsonProperty("lotteryTopicDetail")]
    public object LotteryTopicDetail { get; set; }
}