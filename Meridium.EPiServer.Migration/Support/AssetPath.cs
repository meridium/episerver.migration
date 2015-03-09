using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace Meridium.EPiServer.Migration.Support {
    class AssetPath {
        public AssetPath(IEnumerable<IContent> segments) {
            _segments = segments;
        }

        public ContentFolder AssertPath(ContentReference root) {
            if (Leaf != null) return Leaf;

            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            var rootFolder = repo.Get<ContentFolder>(root);
            var current = rootFolder;
            var segments = new Queue<IContent>(_segments);

            while (segments.Any()) {
                var segment = segments.Dequeue();
                var folder = repo.GetChildren<ContentFolder>(current.ContentLink)
                    .SingleOrDefault(f => f.Name.Equals(segment.Name));
                if (folder == null) {
                    var newFolder = repo.GetDefault<ContentAssetFolder>(current.ContentLink);
                    newFolder.Name = segment.Name;
                    var newRef = repo.Save(newFolder, SaveAction.Publish, AccessLevel.NoAccess);
                    current = repo.Get<ContentFolder>(newRef);
                } else {
                    current = folder;
                }
            }

            Leaf = current;
            return Leaf;
        }

        public ContentFolder Leaf { get; private set; }

        public string Path {
            get { return string.Join("/", _segments.Select(s => s.Name)); }
        }

        public override string ToString() {
            return Path;
        }

        private readonly IEnumerable<IContent> _segments;
    }
}