using AltV.Net;
using NLog;
using NLog.Targets;
using System;

namespace ChoiceVServer.Base {
    [Target("AltV")]
    class NLogAltV : TargetWithLayout {
        protected override void Write(LogEventInfo logEvent) {
            string logMessage = this.Layout.Render(logEvent);
            if(logEvent.Level == LogLevel.Info) { Alt.Core.LogInfo(logMessage); return; }
            if(logEvent.Level == LogLevel.Debug) { ChoiceVAPI.Log(logMessage, ConsoleColor.Blue); return; }
            if(logEvent.Level == LogLevel.Error) { Alt.Core.LogError(logMessage); return; }
            if(logEvent.Level == LogLevel.Fatal) { Alt.Core.LogError(logMessage); return; }
            if(logEvent.Level == LogLevel.Warn) { Alt.Core.LogWarning(logMessage); return; }
            if(logEvent.Level == LogLevel.Trace) { ChoiceVAPI.Log(logMessage, ConsoleColor.Cyan); return; }
            ChoiceVAPI.Log(logMessage, ConsoleColor.Cyan);
        }
    }
}
