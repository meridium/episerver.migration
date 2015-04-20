using System;
using System.IO;
using EPiServer.Core;
using EPiServer.Enterprise;
using EPiServer.Util;

namespace Meridium.EPiServer.Migration.Support {
    class Importer {
        private readonly ContentReference _importRoot;
        private readonly ImportEvents _importEvents;

        public Importer(ContentReference importRoot) {
            _importRoot = importRoot;
            _importEvents = new ImportEvents();
        }

        public virtual void ImportContent(string packagePath, IMigrationLog logger) {
            logger.Log("Import package path: {0}", packagePath);
            logger.Log("Import root page:    {0}", _importRoot);

            _importEvents.Log = logger;

            var assetMigrator = new AssetMigrator(_importRoot).Init();
            var importer = new DataImporter();
            if (string.IsNullOrEmpty(packagePath)) {
                logger.Log("ERROR: PackagePath must be set");
                throw new ArgumentException("PackagePath must be set");
            }
            DataImporter.ContentImporting += _importEvents.DataImporter_ContentImporting;
            DataImporter.ContentImported += _importEvents.DataImporter_ContentImported;
            DataImporter.FileImported += _importEvents.DataImporter_FileImported;
            DataImporter.FileImporting += _importEvents.DataImporter_FileImporting;
            try {
                logger.Log("Opening package");
                using (var fs = new FileStream(packagePath, FileMode.Open, FileAccess.Read)) {
                    importer.Stream = fs;
                    importer.IsTest = false;
                    importer.DestinationRoot = _importRoot;
                    importer.KeepIdentity = false;
                    logger.Log("Executing import");
                    importer.Import();
                }
            }
            catch (Exception e) {
                logger.Log("Error executing export {0}", e);
                throw;
            }
            finally {
                DataImporter.ContentImporting -= _importEvents.DataImporter_ContentImporting;
                DataImporter.ContentImported -= _importEvents.DataImporter_ContentImported;
                DataImporter.FileImported -= _importEvents.DataImporter_FileImported;
                DataImporter.FileImporting -= _importEvents.DataImporter_FileImporting;
            }

            logger.Log("Import done");
            logger.Log("Imported pages: {0}", importer.Log.Status.GetInformationLog(StatusInfo.StatusInfoAction.Imported).Count);
            logger.Log("Imported files: {0}", importer.Log.CountHandledFiles);
            logger.Log("Moving imported assets to site assets");
            assetMigrator.MoveAssetsToSite();
            logger.Log("Assets moved");
           
            logger.Log("-- Import errors --");
            foreach (var error in importer.Log.Errors) {
                logger.Log(error);
            }

            logger.Log("-- Import warnings --");
            foreach (var warning in importer.Log.Warnings) {
                logger.Log(warning);
            }
        }
    }
}