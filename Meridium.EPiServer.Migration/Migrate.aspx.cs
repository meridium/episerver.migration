using System;
using System.IO;
using System.Threading;
using System.Web;
using System.Linq;
using EPiServer.Core;
using log4net;
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

        protected bool MoveAssetsToSite {
            get {
                bool doMoveToSiteAssets;
                if (bool.TryParse(Request.Form["MoveAssetsToSite"], out doMoveToSiteAssets)) {
                    return doMoveToSiteAssets;
                }
                return false;
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
                    Response.End();
                }
                if (!string.IsNullOrEmpty(Request.Form["Import"])) {
                    ValidateInputForImport();
                    ImportContent();
                    Response.End();
                }
                if (!string.IsNullOrEmpty(Request.Form["delete-pagetypes"])) {
                    DeletePageTypes();
                    Response.End();
                }
            }
            catch (ThreadAbortException) {
                Logger.Log("End of transmission");
            }
            catch (AggregateException ex) {
                foreach (var innerException in ex.InnerExceptions) {
                    Logger.Log(innerException.ToString());
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        protected EpiServerDataPackage[] GetPackages() {
            var basePath = HttpContext.Current
                .Server.MapPath("~/migration/packages");

            if(!Directory.Exists(basePath)) 
                return new EpiServerDataPackage[0];

            return new PackageResolver(basePath).GetPackages();
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
            InitLog();

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
            var importer = new Importer(ImportRoot, MoveAssetsToSite);
            InitLog();
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
            InitLog();
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

        internal IMigrationLog Logger { get; set; }

        private void InitLog() {
            Logger = new CompositeMigrationLog(
                new HttpResponseLog(Response),
                new Log4NetLog(Log4netLog));
        }

        private static readonly ILog Log4netLog =
            LogManager.GetLogger(typeof (Migrate));
    }
}