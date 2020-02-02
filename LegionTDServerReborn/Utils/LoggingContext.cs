using System;
using System.Collections.Generic;
using System.Linq;


namespace LegionTDServerReborn.Utils {
    public class LoggingContext : IDisposable {
        private readonly static List<LoggingContext> _context = new List<LoggingContext>();
        private readonly static object _lock = new object();
        public static string CurrentContext {
            get {
                lock(_lock) {
                    var toAdd = _context.Select(c => $"[{c.Prefix}]");
                    return string.Join("", toAdd);
                }
            }
        }
        
        public string Prefix => _prefix ?? _prefixFunc();

        private readonly string _prefix = null;
        private readonly Func<string> _prefixFunc = null;

        static LoggingContext() {
            new LoggingContext(() => $"{DateTime.Now:dd.MM.yy HH:mm}");
        }

        public LoggingContext(string prefix) : this() {
            this._prefix = prefix;
        }

        public LoggingContext(Func<string> prefixFunc) : this() {
            this._prefixFunc = prefixFunc;
        }

        public LoggingContext() {
            lock(_lock) {
                _context.Add(this);
            }
        }

        public void Dispose() {
            lock(_lock) {
                _context.Remove(this);
            }
        }
    }
}
