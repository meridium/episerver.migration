using System;
using EPiServer.Core;

namespace Meridium.EPiServer.Migration.Support {
    public interface IPageMapper {
        string Name { get; }
        Type GetTargetPageType(PageData sourcePage);
        void SetPropertyValues(PageData transformedPage, SourcePage sourcePage);
    }
}