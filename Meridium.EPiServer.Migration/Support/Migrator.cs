﻿using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Framework;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Meridium.EPiServer.Migration.Support {
    class Migrator {
        private readonly IUrlSegmentGenerator _urlSegmentGenerator;
        private readonly IUserImpersonation _userImpersonation;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IContentRepository _repo;
        private readonly IPageMapper _mapper;
        private readonly SourcePage _currentConvertablePageData;
        private readonly ContentReference _root;

        public Migrator(ContentReference root, IPageMapper mapper) {
            _repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _urlSegmentGenerator = ServiceLocator.Current.GetInstance<IUrlSegmentGenerator>();
            _userImpersonation = ServiceLocator.Current.GetInstance<IUserImpersonation>();
            _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            _mapper = mapper;
            _currentConvertablePageData = new SourcePage();
            _root = root;
        }

        IEnumerable<PageData> GetPages() {
            foreach (var pageRef in _repo.GetDescendents(_root).Concat(new[] {_root})) {
                var pages = _repo.GetLanguageBranches<PageData>(pageRef);
                foreach (var pageData in pages) {
                    yield return pageData;
                }
            }
        }

        private IMigrationLog Logger { get; set; }

        public void MigrateContent(IMigrationLog logger) {
            Logger = logger;
            var pages = GetPages().ToList();
            Logger.Log("Found {0} pages to migrate", pages.Count);
            foreach (var page in pages) {
                TransformPage(page);
            }
        }

        private void TransformPage(PageData sourcePage, bool clearPropertyValues = false) {
            MigrationHook.Invoke(new BeforePageTransformEvent(sourcePage), Logger);

            _currentConvertablePageData.Properties = sourcePage.Property;
            _currentConvertablePageData.TypeName = sourcePage.PageTypeName;

            var sourcePageType = _contentTypeRepository.Load(sourcePage.ContentTypeID) as PageType;
            var targetPageType = _contentTypeRepository.Load(_mapper.GetTargetPageType(sourcePage)) as PageType;

            string result;
            //Convert The Page
            if (clearPropertyValues) {
                var keys = new List<KeyValuePair<int, int>>();
                var properties = sourcePage.Property.Select(p => p.PropertyDefinitionID).Where(p => p > 0);
                foreach (var propertyId in properties) {
                    keys.Add(new KeyValuePair<int, int>(propertyId, 0));
                }

                //Convert The Page
                result = PageTypeConverter.Convert(sourcePage.PageLink, sourcePageType, targetPageType, keys, false, false);
            }
            else {
                result = PageTypeConverter.Convert(sourcePage.PageLink, sourcePageType, targetPageType, new List<KeyValuePair<int, int>>(), false, false);
            }
            Logger.Log(result);

            var transformedPage = _repo.Get<PageData>(sourcePage.PageLink, sourcePage.Language).CreateWritableClone();

            _mapper.SetPropertyValues(transformedPage, _currentConvertablePageData);

            transformedPage.URLSegment = _urlSegmentGenerator.Create(transformedPage.URLSegment);
            var oldPrincipal = PrincipalInfo.CurrentPrincipal;
            try {
                PrincipalInfo.CurrentPrincipal = _userImpersonation.CreatePrincipal(sourcePage.ChangedBy);
                ContextCache.Current["PageSaveDB:PageSaved"] = true;    
                var savedPage = _repo.Save(transformedPage, SaveAction.ForceCurrentVersion | SaveAction.Publish | SaveAction.SkipValidation, AccessLevel.NoAccess);

                MigrationHook.Invoke(new AfterPageTransformEvent(savedPage), Logger);
            }
            finally {
                ContextCache.Current["PageSaveDB:PageSaved"] = null;
                PrincipalInfo.CurrentPrincipal = oldPrincipal;
            }
        }
    }
}