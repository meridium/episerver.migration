using System;
using EPiServer.Core;

namespace Meridium.EPiServer.Migration.Support {
    public interface IPageMap {
        void Map(SourcePage source, PageData dest);
        Type GetTargetType(PageData source);
    }
}