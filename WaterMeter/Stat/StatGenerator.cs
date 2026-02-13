using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using Newtonsoft.Json;
using WaterMeter.API;

namespace WaterMeter.Stat
{
    public class StatGenerator(CacheManager cacheManager)
    {
        public async Task<StatResult> RunStatsAsync(string topicId,int totalReplyCountThreshold)
        {
            HashSet<long> replyIds = [];
            Dictionary<int,StatEntry> result = new Dictionary<int,StatEntry>();
            await foreach (var readReply in cacheManager.ReadRepliesAsync(topicId))
            {
                if (readReply.IsDeleted|| replyIds.Contains(readReply.Id)||readReply.UserId==null)
                {
                    continue;
                }

                var userId = readReply.UserId.Value;
                var entry = result.GetValueOrDefault(userId, new StatEntry(userId, 
                    readReply.UserName, int.MaxValue, DateTimeOffset.MaxValue, 0));

                entry.FirstAppearedFloor = Math.Min(entry.FirstAppearedFloor, readReply.Floor);
                entry.TotalReplyCount += 1;
                if (entry.FirstAppearedTime > readReply.Time)
                {
                    entry.FirstAppearedTime = readReply.Time;
                }
                result[userId] = entry;
                replyIds.Add(readReply.Id);
            }

            var entries = result.Values
                .Where(i=>i.TotalReplyCount>=totalReplyCountThreshold)
                .OrderByDescending(i => i.TotalReplyCount)
                .ThenByDescending(i => i.FirstAppearedFloor)
                .ThenByDescending(i => i.UserId)
                .ToList().AsReadOnly();
            return new StatResult(replyIds.Count, entries);
        }
    }
}
