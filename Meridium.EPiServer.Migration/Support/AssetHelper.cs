using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Meridium.EPiServer.Migration.Support {
    static class AssetHelper {
        private static readonly IContentRepository Repo = ServiceLocator.Current.GetInstance<IContentRepository>();
        private static readonly ISiteDefinitionRepository SiteDefinitionRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();

        public static AssetPath GetAssetPath(MediaData file, ContentReference root) {
            var current  = file as IContent;
            var segments = new Stack<IContent>();  
            while (current.ParentLink != root) {
                current = Repo.Get<IContent>(current.ParentLink);
                segments.Push(current);
            }
            return new AssetPath(segments);
        }

        public static ContentFolder GetOrCreateFolder(string name, ContentReference parentLink) {
            var existingFolder = Repo.GetChildren<ContentFolder>(parentLink)
                .FirstOrDefault(f => f.Name.Equals(name));

            if (existingFolder != null) {
                return existingFolder;
            }

            var newFolder = Repo.GetDefault<ContentAssetFolder>(parentLink);
            newFolder.Name = name;
            var publishedNewFolder =
                Repo.Save(newFolder, SaveAction.Publish, AccessLevel.NoAccess);
            return Repo.Get<ContentFolder>(publishedNewFolder);
        }

        public static void MoveFilesTo(this IEnumerable<MediaData> self, ContentFolder destination) {
            foreach (var mediaData in self) {
                var path = GetAssetPath(mediaData, SiteDefinition.Current.GlobalAssetsRoot);
                var destinationFolder = path.AssertPath(destination.ContentLink);
                Repo.Move(mediaData.ContentLink, destinationFolder.ContentLink,
                    AccessLevel.NoAccess, AccessLevel.NoAccess);
            }
        }

        /// <summary>
        /// Gets the site definition of which the specified content is a member
        /// Returns the Current definition when no definition could be determined.
        /// </summary>
        public static SiteDefinition GetSiteDefinition(this ContentReference content) {
            var sites = SiteDefinitionRepository.List();

            var current = Repo.Get<IContent>(content);
            while (current.ContentLink != ContentReference.EmptyReference) {
                var site = sites.FirstOrDefault(s => s.StartPage.Equals(current.ContentLink));
                if (site != null) return site;
                current = Repo.Get<IContent>(current.ParentLink);
            }

            return SiteDefinition.Current;
        }

       
    }
}
