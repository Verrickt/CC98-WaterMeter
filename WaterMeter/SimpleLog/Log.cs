using System.Runtime.CompilerServices;

namespace WaterMeter.SimpleLog;

public record Log(LogLevel Level,string Content, [CallerMemberName] string Name="");