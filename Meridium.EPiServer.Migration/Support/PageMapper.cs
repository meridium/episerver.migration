using System;
using System.Collections.Generic;
using EPiServer.Core;

namespace Meridium.EPiServer.Migration.Support {
    public class PageMapper : IPageMapper {
        public PageMapper(string name) {
            Name = name;
        }

        public PageMapper Map<TPage>(string oldTypeName, params Action<SourcePage, TPage>[] mappers ) where TPage : PageData {
            _maps[oldTypeName] = new PageMap<TPage>(oldTypeName, mappers);
            return this;
        }

        public PageMapper Map(string oldTypeName, IPageMap map) {
            _maps[oldTypeName] = map;
            return this;
        } 

        public PageMapper Map<TPage>(IEnumerable<string> oldTypeNames, params Action<SourcePage, TPage>[] mappers ) where TPage : PageData {
            foreach (var oldTypeName in oldTypeNames) {
                Map(oldTypeName, mappers);
            }
            return this;
        }

        public PageMapper Default<TPage>(params Action<SourcePage, TPage>[] mappers) where TPage : PageData {
            _maps[DefaultMapName] = new PageMap<TPage>(DefaultMapName, mappers);
            return this;
        }

        public Type GetTargetPageType(PageData sourcePage) {
            return _maps.ContainsKey(sourcePage.PageTypeName) 
                ? _maps[sourcePage.PageTypeName].GetTargetType(sourcePage) 
                : _maps[DefaultMapName].GetTargetType(sourcePage);
        }

        public void SetPropertyValues(PageData transformedPage, SourcePage sourcePage) {
            var map = _maps.ContainsKey(sourcePage.TypeName)
                ? _maps[sourcePage.TypeName]
                : _maps[DefaultMapName];

            map.Map(sourcePage, transformedPage);
        }

        public static PageMapper Define(string name) { return new PageMapper(name); }

        public string Name { get; private set; }

        private const string DefaultMapName = "__DEFAULT__";
        private readonly Dictionary<string, IPageMap> _maps =
            new Dictionary<string, IPageMap>();
    }
}