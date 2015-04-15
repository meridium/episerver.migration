using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.DataAccess;
using EPiServer.Enterprise;
using EPiServer.Security;

namespace Meridium.EPiServer.Migration.Support {
    class ImportEvents {
        private OriginalValues _originalValues = null;

        /// <summary>
        /// After the page is imported
        /// </summary>
        public void DataImporter_ContentImported(DataImporter dataImported, ContentImportedEventArgs e) {
            PageData page = null;
            if (!ContentReference.IsNullOrEmpty(e.ContentLink)) {
                page = DataFactory.Instance.Get<PageData>(e.ContentLink).CreateWritableClone();
            }

            if (page == null) {
                return;
            }

            page["PageSaved"] = _originalValues.PageSaved;
            page["PageChanged"] = _originalValues.PageChanged;
            page["PageChangedBy"] = _originalValues.PageChangedBy;
            page["PageCreatedBy"] = _originalValues.PageCreatedBy;
            page["PageChangedOnPublish"] = true;
            PrincipalInfo.CurrentPrincipal = PrincipalInfo.CreatePrincipal(_originalValues.PageChangedBy);
            try {
                global::EPiServer.BaseLibrary.Context.Current["PageSaveDB:PageSaved"] = true;
                DataFactory.Instance.Save(page, SaveAction.ForceCurrentVersion | SaveAction.Publish
                                                | SaveAction.SkipValidation, 
                    AccessLevel.NoAccess);
            }
            finally {
                global::EPiServer.BaseLibrary.Context.Current["PageSaveDB:PageSaved"] = null;
            }
        }

        /// <summary>
        /// Before page is imported
        /// </summary>
        public void DataImporter_ContentImporting(DataImporter dataImporting, ContentImportingEventArgs e) {
            _originalValues = null;
            if (e.TransferContentData is TransferPageData) {
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

        public List<FileImportedEventArgs> ImportedFiles = new List<FileImportedEventArgs>(); 

        public void DataImporter_FileImported(DataImporter dataimported, FileImportedEventArgs e) {
            ImportedFiles.Add(e);
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