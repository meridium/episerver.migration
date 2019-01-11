using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EPiServer.Logging.Compatibility;

namespace Meridium.EPiServer.Migration.Support {
    public static class MapperRegistry {
        private static readonly Dictionary<string, IPageMapper> PageMappers = new Dictionary<string, IPageMapper>();

        public static void Register(params IPageMapper[] mappers) {
            foreach (var pageMapper in mappers) {
                PageMappers[pageMapper.Name] = pageMapper;
            }
        }

        public static IPageMapper Get(string name) {
            return PageMappers.TryGetValue(name, out var mapper) ? mapper : null;
        }

        public static IEnumerable<IPageMapper> Mappers => PageMappers.Values;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationInitializerAttribute : Attribute {}

    static class MigrationInitializer {
        private static readonly object Locker = new object();
        private static bool _isInitialized;
        private static ILog _logger = LogManager.GetLogger(typeof (MapperRegistry));

        public static void FindAndExecuteInitializers(bool force = false) {
            if (_isInitialized && !force) return;

            lock (Locker) {
                if (_isInitialized && !force) return;

                ScanThisFolder();

                _isInitialized = true;
            }
        }

        private static void ScanAndInvokeInitializer( Assembly assembly) {
            try {
                var initializers = assembly.GetTypes()
                    .Where(a => a.GetCustomAttributes<MigrationInitializerAttribute>().Any()).ToList();
                initializers.ForEach(InvokeInitializer);
            }
            catch (Exception ex) {
                _logger.ErrorFormat("Assembly {0} {1}", assembly.FullName, ex.Message);
            }       
        }

        private static void ScanThisFolder() {
            var folder = System.Web.HttpRuntime.BinDirectory;
            var dlls  = Directory.GetFiles(folder, "*.dll")
                .Where( f => !Path.GetFileName(f).StartsWith("System."));

            var exceptions = new List<Exception>();

            foreach (var dll in dlls) {
                try {
                    var assembly = Assembly.LoadFrom(dll);
                    ScanAndInvokeInitializer(assembly);
                }
                catch (Exception e) {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Any()) {
                throw  new AggregateException(
                    "Errors occurred when scanning for migration initializers. " +
                    "See the inner exception list.",
                    exceptions);
            }
        }

        private static void InvokeInitializer(Type initializer) {
            var method = initializer.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
            if (method != null) method.Invoke(null, null);
        }     
    }
}