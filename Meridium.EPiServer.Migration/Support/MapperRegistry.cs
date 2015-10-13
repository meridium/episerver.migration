using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;

namespace Meridium.EPiServer.Migration.Support {
    public static class MapperRegistry {
        public static void Register(params IPageMapper[] mappers) {
            mappers.ForEach( mapper => _mappers[mapper.Name] = mapper);
        }

        public static IPageMapper Get(string name) {
            IPageMapper mapper;
            return _mappers.TryGetValue(name, out mapper) ? mapper : null;
        }

        public static IEnumerable<IPageMapper> Mappers {
            get { return _mappers.Values; }
        } 

        private readonly static Dictionary<string, IPageMapper> _mappers =
            new Dictionary<string, IPageMapper>();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MigrationInitializerAttribute : Attribute {}

    static class MigrationInitializer {
        private static readonly object locker = new object();
        private static string migrationDirectory = "migration";

        public static void FindAndExecuteInitializers(string migrationDir, bool force = false) {
            migrationDirectory = migrationDir;

            if (IsInitialized && !force) return;

            lock (locker) {
                if (IsInitialized && !force) return;

                ScanThisFolder();

                IsInitialized = true;
            }
        }

        private static void ScanAndInvokeInitializer( Assembly assembly) {
            var initializers = assembly.GetTypes().Where(a => a.HasAttribute<MigrationInitializerAttribute>());
            initializers.ForEach(InvokeInitializer);
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
            if (method != null) method.Invoke(null, new object[] {migrationDirectory} );
        }

        private static bool IsInitialized = false;
    }
}