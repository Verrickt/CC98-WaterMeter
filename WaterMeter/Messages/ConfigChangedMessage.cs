using CommunityToolkit.Mvvm.Messaging.Messages;
using WaterMeter.Config;
using WaterMeter.Messages;

namespace WaterMeter.Messages;

public class ConfigChangedMessage(WaterMetterConfig value) : ValueChangedMessage<WaterMetterConfig>(value);