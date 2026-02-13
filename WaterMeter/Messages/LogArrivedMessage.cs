using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterMeter.Messages;

public class LogArrivedMessage(SimpleLog.Log value):ValueChangedMessage<SimpleLog.Log>(value);