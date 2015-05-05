using System;
using System.Collections.Generic;
using System.Web;
using log4net;

namespace Meridium.EPiServer.Migration.Support {

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