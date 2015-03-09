using System.Collections;
using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace Meridium.EPiServer.Migration.Support {
    /// <summary>
    /// Represents the assets under a specified root folder. Is enumerable.
    /// </summary>
    class MediaDataAssets : IEnumerable<MediaData> {
        /// <summary>
        /// Creates and initializes a new MediaDataAssets instance rooted at the specified 
        /// ContentReference. The constructor calls the Refresh method and might take little 
        /// while to complete.
        /// </summary>
        public MediaDataAssets(ContentReference root) {
            _repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _root = root;
            Refresh();
        }

        /// <summary>
        /// Re-reads the MediaData assets under the root by recursivly iterating the substructure
        /// </summary>
        public IEnumerable<MediaData> Refresh() {

            // ?? Any particular reason we don't use the GetDescendants 
            // ?? method of IContentRepository?
            var assets = new List<MediaData>();
            var enumerator = GetDecendants(_root);
            while (enumerator.MoveNext()) {
                assets.Add(enumerator.Current);
            }

            _assets = assets;
            return _assets;
        }

        /// <summary>
        /// Gets an enumerator over all MediaData assets under the root.
        /// </summary>
        public IEnumerator<MediaData> GetEnumerator() {
            return _assets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private IEnumerator<MediaData> GetDecendants(ContentReference parent) {
            // We should be able to use GetDecendants of IContentRepository, aren't we?
            var files = _repo.GetChildren<MediaData>(parent);

            foreach (var mediaData in files) yield return mediaData;

            var folders = _repo.GetChildren<ContentFolder>(parent);

            foreach (var contentFolder in folders) {
                var files2 = GetDecendants(contentFolder.ContentLink);
                while (files2.MoveNext()) {
                    yield return files2.Current;
                }
            }
        }

        private IEnumerable<MediaData> _assets;

        private readonly IContentRepository _repo;
        private readonly ContentReference _root;
    }
}