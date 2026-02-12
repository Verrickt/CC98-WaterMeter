using System.Runtime.CompilerServices;

namespace WaterMeter.Log;

public record Log(LogLevel Level,string Content, [CallerMemberName] string Name="");