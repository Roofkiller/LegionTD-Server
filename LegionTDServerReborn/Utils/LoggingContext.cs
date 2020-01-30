using System;
using System.Linq;


namespace LegionTDServerReborn.Utils {
    public class LoggingContext : IDisposable {
        
        public string Prefix => _prefix ?? _prefixFunc();

        private readonly string _prefix = null;
        private readonly Func<string> _prefixFunc = null;

        public LoggingContext(string prefix) : this() {
            this._prefix = prefix;
        }

        public LoggingContext(Func<string> prefixFunc) : this() {
            this._prefixFunc = prefixFunc;
        }

        public LoggingContext() {
            LoggingUtil.Contexts.Add(this);
        }

        public void Dispose() {
            LoggingUtil.Contexts.Remove(this);
        }
    }
}
