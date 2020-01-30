using System;
using System.Collections.Generic;
using System.Linq;

namespace LegionTDServerReborn.Utils {
    public static class LoggingUtil {
        public static readonly List<LoggingContext> Contexts = new List<LoggingContext>();

        static LoggingUtil() {
            new LoggingContext($"{DateTime.Now:dd.MM.yy HH:mm}");
        }

        private static string _prefix {
            get {
                return string.Join("", Contexts.Select(c => $"[{c.Prefix}]"));
            }
        }

        public static void Log(dynamic toLog) {
            Console.WriteLine($"{_prefix} {toLog}");
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