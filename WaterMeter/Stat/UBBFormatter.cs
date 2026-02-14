using System.Text;
using WaterMeter.API;

namespace WaterMeter.Stat;

public class UBBFormatter(OverWatchContext context)
{


    private static void WriteTableHeader(StringBuilder sb)
    {
        static void WriteHeaderCell(StringBuilder sb, string name)
        {
            sb.Append("[th]");
            sb.Append(name);
            sb.Append("[/th]");
        }
        WriteHeaderCell(sb, "用户名");
        WriteHeaderCell(sb, "回复数");
        WriteHeaderCell(sb, "首次出现楼层");
        WriteHeaderCell(sb, "首次出现时间");
        sb.AppendLine();
    }

    private static void WriteRow(StringBuilder sb, StatEntry entry,TopicInfo topicInfo)
    {
        void WriteCell(StringBuilder sb, string content)
        {
            sb.Append("[td]");
            sb.Append(content);
            sb.Append("[/td]");
        }
        sb.Append("[tr]");
        WriteCell(sb,$"[user]{entry.UserName}[/user]");
        WriteCell(sb,entry.TotalReplyCount.ToString());
        WriteCell(sb,$"[url=/topic/{topicInfo.Id}/{((entry.FirstAppearedFloor-1)/10+1)}#{(entry.FirstAppearedFloor-1)%10+1}]{entry.FirstAppearedFloor}[/url]");
        WriteCell(sb,entry.FirstAppearedTime.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("[/tr]");
    }
    public string Format(StatResult result)
    {
        var topicInfo = context.CachedTopicInfo;
        var sb = new StringBuilder();
        if (result.AlmostAdults.Any())
        {
            sb.AppendLine($"[center]「[topic={topicInfo.Id}]{topicInfo.Title}[/topic]」成年&即将成年榜 by [url=https://github.com/verrickt/CC98-WaterMeter]CC98水表助手[/url][/center]");
            sb.AppendLine($"[right]——生成时间 {DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")} 统计回复数:{result.TotalReplies},成年用户数:{result.Adults.Count},即将成年用户数:{result.AlmostAdults.Count}[/right]");
        }
        else
        {
            sb.AppendLine($"[center]「[topic={topicInfo.Id}]{topicInfo.Title}[/topic]」成年榜 by [url=https://github.com/verrickt/CC98-WaterMeter]CC98水表助手[/url][/center]");
            sb.AppendLine($"[right]——生成时间 {DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")} 统计回复数:{result.TotalReplies},成年用户数:{result.Adults.Count}[/right]");
        }

        sb.AppendLine("[table]");
        //生成表头
        WriteTableHeader(sb);
        foreach (var statResult in result.Adults)
        {
            WriteRow(sb,statResult,topicInfo);
        }
        sb.AppendLine("[/table]");
        if (result.AlmostAdults.Count > 0) 
        { 
            sb.AppendLine($"[center]即将成年榜[/center]");

            sb.AppendLine("[table]");
            WriteTableHeader(sb);
            foreach (var statResult in result.AlmostAdults)
            {
                WriteRow(sb,statResult,topicInfo);
            }
            sb.AppendLine("[/table]");
        }
        return sb.ToString();
    }
}