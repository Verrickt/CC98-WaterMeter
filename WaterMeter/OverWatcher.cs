using CommunityToolkit.Mvvm.Messaging;
using WaterMeter.API;
using WaterMeter.SimpleLog;
using WaterMeter.Messages;

namespace WaterMeter;

public class OverWatcher(CC98API api, OverWatchContext context)
{
    private Task? _overWatchTask;
    private int _started = 0;
    public bool IsRunning => _started != 0;
    private async Task OverwatchImpl()
    {
        DateTimeOffset prev = DateTimeOffset.MinValue;
        new SimpleLog.Log(LogLevel.Info, "获取帖子信息....").Send();
        var topicInfo = await api.GetTopicInfoAsync(context.TopicId);
        await context.UpdateTopicInfoAsync(topicInfo);
        while (_started == 1)
        {

            var now = DateTimeOffset.Now;
            var diff = now - prev;
            if (diff.Seconds >= context.OverwatchInterval)
            {
                try
                {
                    if (context.IsFloorCached(context.CurrentFloor))
                    {
                        var nextUncached = context.ToNextUncachedFloor();
                        new SimpleLog.Log(LogLevel.Info, $"楼层{context.CurrentFloor}命中缓存，跳转到{nextUncached}层").Send();
                        await context.AdvanceAsync(nextUncached - context.CurrentFloor);
                    }
                    if (context.CurrentFloor <= context.MaxFloor)
                    {

                        var postInfos = await api.GetPostInfoAsync(context.TopicId, context.CurrentFloor);
                        await context.UpdateReplyInfoAsync(postInfos);
                        await context.AdvanceAsync(postInfos?.Count);
                    }
                    else
                    {
                        new SimpleLog.Log(LogLevel.Info, "已到楼顶...10s后重新获取帖子信息").Send();
                        await Task.Delay(10000);
                        topicInfo = await api.GetTopicInfoAsync(context.TopicId);
                        await context.UpdateTopicInfoAsync(topicInfo);
                    }
                }
                catch (Exception e) when (ExceptionFilterUtility.False(() => new SimpleLog.Log(
                                              LogLevel.Critical, $"调用CC98API错误:{e}").Send()))
                {
                }
                finally
                {
                    prev = now;
                }
            }
            else
            {
                await Task.Delay(1000);
            }
        }
    }
    public void StartOverWatch()
    {
        if (_overWatchTask == null || _overWatchTask.IsFaulted || _overWatchTask.IsCompleted)
        {
            _overWatchTask = Task.Run(async () =>
            {
                try
                {
                    await OverwatchImpl();
                }
                catch (Exception e) when (ExceptionFilterUtility.True(() => new SimpleLog.Log(LogLevel.Critical, $"守望出错{e}").Send()))
                {
                    return;
                }
            });
        }
        Interlocked.Exchange(ref _started, 1);
        new SimpleLog.Log(LogLevel.Info, "开始守望").Send();
    }

    public void StopOverWatch()
    {
        Interlocked.Exchange(ref _started, 0);
        new SimpleLog.Log(LogLevel.Info, "守望已停止").Send();
    }
}