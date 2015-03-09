using System.Linq;
using EPiServer.Core;

namespace Meridium.EPiServer.Migration.Support {
    public class SourcePage {
        public string TypeName { get; set; }
        public PropertyDataCollection Properties { get; set; }

        public TValue GetValue<TValue>(string propertyName, TValue @default = default(TValue)) where TValue : class {
            var data = Properties != null ? Properties.Get(propertyName) : null;
            return (data != null) ? (data.Value as TValue) : @default;
        }

        public TValue GetValueWithFallback<TValue>(params string[] properties) where TValue : class {
            var property = properties.SkipWhile(p => null == Properties.Get(p)).FirstOrDefault();
            return (property != null) ? GetValue<TValue>(property) : null;
        }
    }
}