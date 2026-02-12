using Newtonsoft.Json;

namespace WaterMeter.API;

public class PostInfo
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("parentId")]
    public long ParentId { get; set; }

    [JsonProperty("boardId")]
    [JsonIgnore]
    public int BoardId { get; set; }

    [JsonProperty("userName")]
    public string UserName { get; set; }

    [JsonProperty("userId")]
    public int? UserId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("content")]
    [JsonIgnore]
    public string Content { get; set; }

    [JsonProperty("time")]
    public DateTimeOffset Time { get; set; }

    [JsonProperty("topicId")]
    public int TopicId { get; set; }

    [JsonProperty("ip")]
    [JsonIgnore]

    public string Ip { get; set; }

    [JsonProperty("state")]
    [JsonIgnore]

    public int State { get; set; }

    [JsonProperty("isAnonymous")]

    public bool IsAnonymous { get; set; }

    [JsonProperty("floor")]
    public int Floor { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("allowedViewers")]
    [JsonIgnore]

    public object AllowedViewers { get; set; }

    [JsonProperty("isAllowedOnly")]
    [JsonIgnore]

    public bool IsAllowedOnly { get; set; }

    [JsonProperty("contentType")]
    [JsonIgnore]

    public int ContentType { get; set; }

    [JsonProperty("lastUpdateTime")]
    public DateTimeOffset? LastUpdateTime { get; set; }

    [JsonProperty("lastUpdateAuthor")]
    public string LastUpdateAuthor { get; set; }

    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; }

    [JsonProperty("likeCount")]
    [JsonIgnore]

    public int LikeCount { get; set; }

    [JsonProperty("dislikeCount")]
    [JsonIgnore]

    public int DislikeCount { get; set; }

    [JsonProperty("isLZ")]
    [JsonIgnore]

    public bool IsLz { get; set; }

    [JsonProperty("likeState")]
    public int LikeState { get; set; }

    [JsonProperty("awards")]
    [JsonIgnore]

    public List<object> Awards { get; set; }

    [JsonProperty("isMe")]
    [JsonIgnore]

    public bool IsMe { get; set; }
}