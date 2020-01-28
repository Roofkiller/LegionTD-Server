using System;

namespace LegionTDServerReborn.Utils {
    public static class LoggingUtil {
        public static void Log(dynamic toLog) {
            Console.WriteLine($"[{DateTime.Now:dd.MM.yy HH:mm}] {toLog}");
        }

        public static void Warn(dynamic toLog) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log(toLog);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Error(dynamic toLog) {
            Console.ForegroundColor = ConsoleColor.Red;
            Log(toLog);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}