using System.Runtime.InteropServices;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Meridium.EPiServer.Migration.Support {
    class AssetMigrator {
        public AssetMigrator(ContentReference importRoot) {
            _importRoot = importRoot;
            _assetDiffer = new AssetDiffer(SiteDefinition.Current.GlobalAssetsRoot);
        }

        public AssetMigrator Init() {
            _assetDiffer.TakeSnapshot();
            return this;
        }

        public void MoveAssetsToSite() {
            var fileDiff = _assetDiffer.TakeSnapshot().GetDiff();
            var destinationFolder = GetDestinationFolder();
            fileDiff.MoveFilesTo(destinationFolder);
            ApplyAccessControl(destinationFolder);
        }

        private void ApplyAccessControl(ContentFolder folder) {
            var clone  = folder.CreateWritableClone();
            var acl = clone.GetContentSecurityDescriptor() as ContentAccessControlList;

            if (acl == null) return;

            var repo = ServiceLocator.Current.GetInstance<IContentSecurityRepository>();
            repo.Save(clone.ContentLink, acl, SecuritySaveType.ReplaceChildPermissions);
        }

        private ContentFolder GetDestinationFolder() {
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();

            var siteAssetsFolder = repo.Get<ContentFolder>( _importRoot.GetSiteDefinition().SiteAssetsRoot);
            var destinationFolder = AssetHelper.GetOrCreateFolder("Migrerade filer", siteAssetsFolder.ContentLink);

            return destinationFolder;
        }

        readonly ContentReference _importRoot;
        readonly AssetDiffer _assetDiffer;
    }
}