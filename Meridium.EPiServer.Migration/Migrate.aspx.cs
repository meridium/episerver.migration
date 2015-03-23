using System;
using EPiServer.Core;
using Meridium.EPiServer.Migration.Support;

namespace Meridium.EPiServer.Migration {
    public partial class Migrate : System.Web.UI.Page {
        
        protected ContentReference StartPageId {
            get {
                ContentReference pageRef;
                if (ContentReference.TryParse(Request.Form["StartPageId"], out pageRef)) {
                    return pageRef;
                }
                return null;
            }
        }

        protected ContentReference ImportRoot {
            get {
                ContentReference pageRef;
                if (ContentReference.TryParse(Request.Form["UploadTarget"], out pageRef)) {
                    return pageRef;
                }
                return null;
            }
        }

        protected string ImportPackagePath {
            get { return Request.Form["ImportPackage"] ?? ""; }
        }

        protected IPageMapper SelectedPageMapper {
            get {
                var mapperName = Request.Form["Mapper"] ?? "";
                return MapperRegistry.Get(mapperName);
            }
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);
            Load += Page_Load;
            Server.ScriptTimeout = int.MaxValue;
        }

        protected void Page_Load(object sender, EventArgs e) {
            try {
                MigrationInitializer.FindAndExecuteInitializers();
                if (!string.IsNullOrEmpty(Request.Form["Run"])) {
                    ValidateInputForMigration();
                    MigrateContent();
                }
                if (!string.IsNullOrEmpty(Request.Form["Import"])) {
                    ValidateInputForImport();
                    ImportContent();
                }
                if (!string.IsNullOrEmpty(Request.Form["delete-pagetypes"])) {
                    DeletePageTypes();
                }
            }
            catch (AggregateException ex) {
                Logger = new MigrationLogger();
                Logger.Log("!!! Error in migration !!!");
                foreach (var innerException in ex.InnerExceptions) {
                    Logger.Log(innerException.ToString());
                }
            }
            catch (Exception ex) {
                Logger = new MigrationLogger();
                Logger.Log("!!! Error in migration !!!");
                Logger.Log(ex.ToString());
            }
        }


        protected void ValidateInputForMigration() {
            if (ContentReference.IsNullOrEmpty(StartPageId)) {
                throw new ArgumentException("StartPageId cannot be null");
            }

            if (SelectedPageMapper == null) {
                throw new ArgumentException("Must select a page mapper");
            }
        }

        protected void ValidateInputForImport() {
            if (ContentReference.IsNullOrEmpty(ImportRoot)) {
                throw new ArgumentException("UploadTarget cannot be null");
            }
            if (ImportPackagePath == "") {
                throw new ArgumentException("Import package path must not be null");
            }
        }

        protected void DeletePageTypes() {
            var loggingOnly = Request.Form["logging-only"] != null;
            Logger = new MigrationLogger();
            Logger.Log("Starting page type deletion @ {0:HH:mm:ss}", DateTime.Now);
            var deleter = new ImportedPageTypeDeleter(Logger);
            try {
                deleter.DeletePageTypes(loggingOnly);
            }
            catch (Exception e) {
                Logger.Log("!!! PAGE TYPE DELETION FAILED !!!");
                Logger.Log(e.ToString());
            }
            finally {
                Logger.Log("Page type deletion done @ {0:HH:mm:ss}", DateTime.Now);
            }
        }

        protected void ImportContent() {
            var importer = new Importer(ImportRoot);
            Logger = new MigrationLogger();
            Logger.Log("Starting import @ {0:HH:mm:ss}", DateTime.Now);
            try {
                importer.ImportContent(ImportPackagePath, Logger);
            }
            catch (Exception e) {
                Logger.Log("!!! IMPORT FAILED !!!");
                Logger.Log(e.ToString());
            }
            finally {
                Logger.Log("Import done @ {0:HH:mm:ss}", DateTime.Now);
            }
        }

        protected void MigrateContent() {
            var migrator = new Migrator(StartPageId, SelectedPageMapper);
            Logger = new MigrationLogger();
            Logger.Log("Starting migration @ {0:HH:mm:ss}", DateTime.Now);
            try {
                migrator.MigrateContent(Logger);
            }
            catch (Exception e) {
                Logger.Log("!!! MIGRATION FAILED !!!");
                Logger.Log(e.ToString());
            }
            finally {
                Logger.Log("Migration done @ {0:HH:mm:ss}", DateTime.Now);
            }
        }

        internal MigrationLogger Logger { get; set; }

        protected string DisplayLog() {
            if (Logger == null) return "";

            return string.Format("<h2>Log</h2><div class='row log-output'><pre>{0}</pre></div>", Logger);
        }
    }
}