using System;
using System.IO;
using EPiServer.Core;
using EPiServer.Enterprise;
using EPiServer.ServiceLocation;
using EPiServer.Util;

namespace Meridium.EPiServer.Migration.Support {
    class Importer {
        private readonly ContentReference _importRoot;
        private readonly ImportEvents _importEvents;
        private readonly IDataImportEvents _dataImportEvents;
        private readonly IDataImporter _dataImporter;

        public Importer(ContentReference importRoot) {
            _importRoot = importRoot;
            _importEvents = new ImportEvents();
            _dataImportEvents = ServiceLocator.Current.GetInstance<IDataImportEvents>();
            _dataImporter = ServiceLocator.Current.GetInstance<IDataImporter>();
        }

        public virtual void ImportContent(string packagePath, IMigrationLog logger) {
            logger.Log("Import package path: {0}", packagePath);
            logger.Log("Import root page:    {0}", _importRoot);

            _importEvents.Log = logger;

            var assetMigrator = new AssetMigrator(_importRoot).Init();
            
            if (string.IsNullOrEmpty(packagePath)) {
                logger.Log("ERROR: PackagePath must be set");
                throw new ArgumentException("PackagePath must be set");
            }
            _dataImportEvents.ContentImporting += _importEvents.DataImporter_ContentImporting;
            _dataImportEvents.ContentImported += _importEvents.DataImporter_ContentImported;
            _dataImportEvents.BlobImported += _importEvents.DataImporter_FileImported;
            _dataImportEvents.BlobImporting += _importEvents.DataImporter_FileImporting;
            try {
                logger.Log("Opening package");
                var fs = new FileStream(packagePath, FileMode.Open, FileAccess.Read);
                var importOptions = new ImportOptions {
                    KeepIdentity = true,
                    IsTest = false
                };
                _dataImporter.Import(fs, _importRoot, importOptions);
            }
            catch (Exception e) {
                logger.Log("Error executing export {0}", e);
                throw;
            }
            finally {
                _dataImportEvents.ContentImporting -= _importEvents.DataImporter_ContentImporting;
                _dataImportEvents.ContentImported -= _importEvents.DataImporter_ContentImported;
                _dataImportEvents.BlobImported -= _importEvents.DataImporter_FileImported;
                _dataImportEvents.BlobImporting -= _importEvents.DataImporter_FileImporting;
            }

            logger.Log("Import done");
            logger.Log("Imported pages: {0}", _dataImporter.Status.Log.Status.GetInformationLog(StatusInfo.StatusInfoAction.Imported).Count);
            //logger.Log("Imported files: {0}",  _dataImporter.Status. importer.Log.CountHandledFiles);
            logger.Log("Moving imported assets to site assets");
            assetMigrator.MoveAssetsToSite();
            logger.Log("Assets moved");
           
            logger.Log("-- Import errors --");
            foreach (var error in _dataImporter.Status.Log.Errors) {
                logger.Log(error);
            }

            logger.Log("-- Import warnings --");
            foreach (var warning in _dataImporter.Status.Log.Warnings) {
                logger.Log(warning);
            }
        }
    }
}