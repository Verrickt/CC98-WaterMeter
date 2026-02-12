using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using Newtonsoft.Json;
using WaterMeter.API;

namespace WaterMeter.Stat
{
    public class StatGenerator(OverWatchContext context)
    {
        public async Task<StatResult> RunStatsAsync(int totalReplyCountThreshold)
        {
            HashSet<long> replyIds = new HashSet<long>();
            await using var stream = File.OpenRead(context.ReplyCachePath);
            using var sr = new StreamReader(stream);
            Dictionary<int,StatEntry> result = new Dictionary<int,StatEntry>();
            var totalReplies = 0;
            while (true)
            {
                var line = await sr.ReadLineAsync();
                if (line == null)
                {
                    break;
                }
                var replies = JsonConvert.DeserializeObject<PostInfo[]>(line)!
                    .Where(i=>!replyIds.Contains(i.Id)).Where(i=>!i.IsDeleted)??[];
                var postInfos = replies.ToList();
                totalReplies += postInfos.Count();
                foreach (var group in postInfos.GroupBy(i=>i.UserId!.Value))
                {
                    var replyCount = group.Count();
                    var userName = group.Select(i => i.UserName).FirstOrDefault()!;
                    group.Select(i => i.Id).ToList().ForEach(i => replyIds.Add(i));
                    var firstAppearedFloor = group.Min(i => i.Floor);
                    var firstAppearedTime = group.Min(i => i.Time);
                    var entry = result.GetValueOrDefault(group.Key, new StatEntry(group.Key, userName, int.MaxValue, DateTimeOffset.MaxValue,0));
                    entry.TotalReplyCount += replyCount;
                    entry.FirstAppearedFloor = Math.Min(entry.FirstAppearedFloor, firstAppearedFloor);
                    if (entry.FirstAppearedTime > firstAppearedTime)
                    {
                        entry.FirstAppearedTime = firstAppearedTime;
                    }
                    result[group.Key] = entry;
                }
            }

            var entries = result.Values
                .Where(i=>i.TotalReplyCount>=totalReplyCountThreshold)
                .OrderByDescending(i => i.TotalReplyCount)
                .ThenByDescending(i => i.FirstAppearedFloor)
                .ThenByDescending(i => i.UserId)
                .ToList().AsReadOnly();
            return new StatResult(replyIds.Count(), entries);
        }
    }
}
