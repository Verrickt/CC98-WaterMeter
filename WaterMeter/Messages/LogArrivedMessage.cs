using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterMeter.Messages;

public class LogArrivedMessage(Log.Log value):ValueChangedMessage<Log.Log>(value);