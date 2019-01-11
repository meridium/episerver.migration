using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Transfer.Internal;
using EPiServer.DataAccess;
using EPiServer.Enterprise;
using EPiServer.Enterprise.Transfer;
using EPiServer.Framework;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace Meridium.EPiServer.Migration.Support {
    class ImportEvents {
        private readonly IContentRepository _contentRepository;
        private readonly IUserImpersonation _userImpersonation;

        private OriginalValues _originalValues = null;
        public IMigrationLog Log { get; set; }

        public ImportEvents() {
            _contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            _userImpersonation = ServiceLocator.Current.GetInstance<IUserImpersonation>();
        }

        /// <summary>
        /// After the page is imported
        /// </summary>
        public void DataImporter_ContentImported(ITransferContext transferContext, ContentImportedEventArgs e) {
            PageData page = null;
            if (!ContentReference.IsNullOrEmpty(e.ContentLink)) {
                page = _contentRepository.Get<PageData>(e.ContentLink).CreateWritableClone();
            }

            if (page == null) {
                return;
            }

            page["PageSaved"] = _originalValues.PageSaved;
            page["PageChanged"] = _originalValues.PageChanged;
            page["PageChangedBy"] = _originalValues.PageChangedBy;
            page["PageCreatedBy"] = _originalValues.PageCreatedBy;
            page["PageChangedOnPublish"] = true;
            PrincipalInfo.CurrentPrincipal = _userImpersonation.CreatePrincipal(_originalValues.PageChangedBy);
            try {
                ContextCache.Current["PageSaveDB:PageSaved"] = true;
                _contentRepository.Save(page, SaveAction.ForceCurrentVersion | SaveAction.Publish | SaveAction.SkipValidation, AccessLevel.NoAccess);
            }
            catch {
                ContextCache.Current["PageSaveDB:PageSaved"] = null;
            }

            MigrationHook.Invoke(new AfterPageImportEvent(e), Log);
        }

        /// <summary>
        /// Before page is imported
        /// </summary>
        public void DataImporter_ContentImporting(ITransferContext transferContext, ContentImportingEventArgs e) {
            _originalValues = null;
            if (e.TransferContentData is TransferPageData) {
                MigrationHook.Invoke(new BeforePageImportEvent(e), Log);

                var externalUrl =
                    e.TransferContentData.RawContentData.Property.FirstOrDefault(x => x.Name == "PageExternalURL");
                if (externalUrl != null && !string.IsNullOrEmpty(externalUrl.Value)) {
                    externalUrl.Value = null;
                }
                // Todo: Do we need this? Should be handled by property mappings
                RemoveDC(e, "MainBody");
                _originalValues = new OriginalValues {
                    PageSaved = DateTime.Parse(GetValue(e, "PageSaved")),
                    PageChanged = DateTime.Parse(GetValue(e, "PageChanged")),
                    PageChangedBy = GetValue(e, "PageChangedBy"),
                    PageCreatedBy = GetValue(e, "PageCreatedBy"),
                    PageGuid = Guid.Parse(GetValue(e, "PageGUID"))
                };
            }
        }

        public void DataImporter_FileImported(ITransferContext transferContext, FileImportedEventArgs e) {
            MigrationHook.Invoke(new AfterFileImportEvent(e), Log);
        }

        public void DataImporter_FileImporting(ITransferContext transferContext, FileImportingEventArgs e) {
            MigrationHook.Invoke(new BeforeFileImportEvent(e), Log);
        }

        private string GetValue(ContentImportingEventArgs e, string propertyName) {
            var property = e.TransferContentData.RawContentData.Property.FirstOrDefault(x => x.Name == propertyName);
            return property != null ? property.Value : string.Empty;
        }

        private void RemoveDC(ContentImportingEventArgs e, string propertyName) {
            var property = e.TransferContentData.RawContentData.Property.FirstOrDefault(x => x.Name == propertyName);
            if (property != null) {
                property.Value = StringExtensions.CleanupForMainBody((string) property.Value);
            }
        }

        public class OriginalValues {
            public DateTime PageSaved { get; set; }
            public DateTime PageChanged { get; set; }
            public string PageChangedBy { get; set; }
            public string PageCreatedBy { get; set; }
            public Guid PageGuid { get; set; }
        }
    }
}