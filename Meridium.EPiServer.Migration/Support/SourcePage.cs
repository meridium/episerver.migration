using System.Linq;
using EPiServer.Core;

namespace Meridium.EPiServer.Migration.Support {
    public class SourcePage {
        public string TypeName { get; set; }
        public PropertyDataCollection Properties { get; set; }

        public TValue GetValue<TValue>(string propertyName, TValue @default = default(TValue)) {
            var data = Properties != null ? Properties.Get(propertyName) : null;

            if (data != null && data.Value is TValue)
                return (TValue) data.Value;

            return @default;
        }

        public TValue GetValueWithFallback<TValue>(params string[] properties) {
            var property = properties.SkipWhile(p => !Properties.HasValue(p)).FirstOrDefault();
            return (property != null) ? GetValue<TValue>(property) : default(TValue);
        }
    }

    internal static class PropertyDataExtensions {
        public static bool HasValue(this PropertyDataCollection self, string key) {
            var property = self.Get(key);

            if (property == null) return false;

            return !(property.IsNull || string.IsNullOrWhiteSpace(property.ToString()));
        }
    }
}