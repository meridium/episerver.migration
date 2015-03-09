using System;
using Castle.Core.Internal;
using EPiServer.Core;

namespace Meridium.EPiServer.Migration.Support {
    public class PageMap<TPage> : IPageMap where TPage : PageData {
        public PageMap(string sourcePageTypeName, Action<SourcePage, TPage>[] propertySetters) {
            SourcePageTypeName = sourcePageTypeName;
            _propertySetters   = propertySetters ?? new Action<SourcePage, TPage>[0];
        }

        public Type GetTargetType(PageData sourcePage) {
            return typeof (TPage);
        }

        public void Map(SourcePage source, PageData dest) {
            Map(source, (TPage) dest);
        }

        public void Map(SourcePage source, TPage newPage) {
            _propertySetters.ForEach( map => map(source, newPage) );
        }

        public string SourcePageTypeName { get; private set; }
        private readonly Action<SourcePage, TPage>[] _propertySetters;
    }
}