using CommunityToolkit.Mvvm.Messaging;
using WaterMeter.Log;
using WaterMeter.Messages;

namespace WaterMeter.Log;

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