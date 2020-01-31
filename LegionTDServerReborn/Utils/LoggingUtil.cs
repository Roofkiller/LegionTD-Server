using System;
using System.Collections.Generic;
using System.Linq;

namespace LegionTDServerReborn.Utils {
    public static class LoggingUtil {
        public static List<LoggingContext> Contexts {
            get {
                return _contexts ?? (_contexts = new List<LoggingContext>());
            }
        }
        public static List<LoggingContext> _contexts = new List<LoggingContext>();

        private static string _prefix {
            get {
                var result = $"{DateTime.Now:dd.MM.yy HH:mm}";
                try {
                    if (Contexts == null) {
                        Console.WriteLine("The Contexts field of LoggingUtil is null!");
                    } else {
                        var toAdd = Contexts.Select(c => $"[{c.Prefix}]");
                        result += string.Join("", toAdd);
                    }
                } catch (Exception) {
                    Console.WriteLine("Something is wrong with the LoggingContext we dispose the old ones.");
                    _contexts = new List<LoggingContext>();
                }
                return result;
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