using System.Collections.Generic;

namespace Meridium.EPiServer.Migration.Support {
    class MigrationLogger {
        public void Log(string messageFormat, params object[] args) {
            _log.Add(string.Format(messageFormat, args));
        }

        public IEnumerable<string> Entries { get { return _log; } }

        public override string ToString() {
            return ToString("\r\n> ");
        }

        public string ToString(string separator) {
            return string.Join(separator, Entries);
        }

        private readonly List<string> _log = new List<string>();
    }
}