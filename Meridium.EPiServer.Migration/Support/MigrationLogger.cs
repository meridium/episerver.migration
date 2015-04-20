using System;
using System.Collections.Generic;
using System.Web;
using log4net;

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

    public interface IMigrationLog {
        void Log(string format, params object[] args);
    }

    class CompositeMigrationLog : IMigrationLog {
        public CompositeMigrationLog(params IMigrationLog[] logs) {
            this.logs = logs;
        }

        public void Log(string format, params object[] args) {
            foreach (var migrationLog in logs) {
                migrationLog.Log(format, args);
            }
        }

        private readonly IMigrationLog[] logs;
    }


    class HttpResponseLog : IMigrationLog {
        public HttpResponseLog( HttpResponse httpResponse ) {
            this.httpResponse = httpResponse;
        }

        public void Log(string format, params object[] args) {
            var message = string.Format(format, args);
            var lineFormat = "> {0:HH:mm:ss.fff} - {1}\r\n";

            httpResponse.Output.Write(lineFormat, DateTime.Now, message);
            httpResponse.Flush();
        }

        private readonly HttpResponse httpResponse;
    }


    class Log4NetLog : IMigrationLog {
        public Log4NetLog(ILog log) {
            this.log = log;
        }

        public void Log(string format, params object[] args) {
            log.Info(string.Format(format, args));
        }

        private readonly ILog log;
    }

    class NothingMigrationLogger : IMigrationLog {
        public void Log(string format, params object[] args) {}
    }
}