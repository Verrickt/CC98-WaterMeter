using CommunityToolkit.Mvvm.Messaging;
using WaterMeter.SimpleLog;
using WaterMeter.Messages;

namespace WaterMeter.SimpleLog;

public static class LogExtensions
{
    extension(Log log)
    {
        public void Send()
        {
            WeakReferenceMessenger.Default.Send(new LogArrivedMessage(log));
        }
    }
}