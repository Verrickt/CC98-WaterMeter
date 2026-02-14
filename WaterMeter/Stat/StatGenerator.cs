using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using Newtonsoft.Json;
using WaterMeter.API;
using WaterMeter.SimpleLog;

namespace WaterMeter.Stat
{
    public class StatGenerator(CacheManager cacheManager)
    {
        public async Task<StatResult> RunStatsAsync(string topicId,int adultThreshold,int almostAdultThreshold)
        {

            HashSet<int> replyFloors = [];
            HashSet<int> deletedReplies = [];
            int maxFloor = 0;
            Dictionary<int,StatEntry> result = new Dictionary<int,StatEntry>();
            await foreach (var reply in cacheManager.ReadRepliesAsync(topicId))
            {
                if (reply.IsDeleted||reply.UserId==null)
                {
                    deletedReplies.Add(reply.Floor);
                    continue;
                }
                if (replyFloors.Contains(reply.Floor)) { continue; }
                maxFloor = Math.Max(maxFloor, reply.Floor);
                var userId = reply.UserId.Value;
                var entry = result.GetValueOrDefault(userId, new StatEntry(userId, 
                    reply.UserName, int.MaxValue, DateTimeOffset.MaxValue, 0));

                entry.FirstAppearedFloor = Math.Min(entry.FirstAppearedFloor, reply.Floor);
                entry.TotalReplyCount += 1;
                if (entry.FirstAppearedTime > reply.Time)
                {
                    entry.FirstAppearedTime = reply.Time;
                }
                result[userId] = entry;
                replyFloors.Add(reply.Floor);
            }
            var list = replyFloors.Union(deletedReplies).OrderBy(i => i).ToList();
            var hasGap = list.Count() != maxFloor;
            var adults = result.Values
                .Where(i=>i.TotalReplyCount>=adultThreshold)
                .OrderByDescending(i => i.TotalReplyCount)
                .ThenByDescending(i => i.FirstAppearedFloor)
                .ThenByDescending(i => i.UserId)
                .ToList().AsReadOnly();
            var almostAdults = new List<StatEntry>().AsReadOnly();
            if (almostAdultThreshold != 0)
            {
                almostAdults = result.Values
                .Where(i => i.TotalReplyCount >= almostAdultThreshold && i.TotalReplyCount < adultThreshold)
                .OrderByDescending(i => i.TotalReplyCount)
                .ThenByDescending(i => i.FirstAppearedFloor)
                .ThenByDescending(i => i.UserId)
                .ToList().AsReadOnly();
            }
            if (hasGap)
            {
                new Log(LogLevel.Warning, "数据存在空隙，结果存在误差。请使用[补全楼层空隙]功能以消除误差").Send();
            }
            return new StatResult(replyFloors.Count,deletedReplies.Count,hasGap,adults,almostAdults);
        }
    }
}
