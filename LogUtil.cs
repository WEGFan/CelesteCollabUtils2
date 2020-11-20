using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CollabUtils2 {
    public static class LogUtil {
        public static void Log(string text, LogLevel logLevel = LogLevel.Verbose) {
            Logger.Log(logLevel, "CollabUtils2", text);
#if DEBUG
            Color color;
            switch (logLevel) {
                case LogLevel.Warn:
                    color = Color.Yellow;
                    break;
                case LogLevel.Error:
                    color = Color.Red;
                    break;
                default:
                    color = Color.Cyan;
                    break;
            }
            try {
                Engine.Commands?.Log($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CollabUtils2] {logLevel}: {text}", color);
            } catch (Exception err) {
                // ignored
            }
#endif
        }
    }
}