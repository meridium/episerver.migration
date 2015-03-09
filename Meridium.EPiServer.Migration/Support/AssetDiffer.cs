using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace Meridium.EPiServer.Migration.Support {
    /// <summary>
    /// Takes snapshots of the assets `MediaData` under the specified root folder.
    /// </summary>
    class AssetDiffer {
        /// <summary>
        /// Creates a new differ rooted at the specified ContentReference
        /// </summary>
        public AssetDiffer(ContentReference root) {
            _assets = new MediaDataAssets(root);
        }

        /// <summary>
        /// Gets the MediaData in the latest snapshot which are NOT present in the first.
        /// </summary>
        public IEnumerable<MediaData> GetDiff() {
            if (_baseLine == null || _latest == null)
                return Enumerable.Empty<MediaData>();

            return _latest.Except(_baseLine, new MediaDataComparer());
        }

        /// <summary>
        /// Takes a snapshot of the MediaData under the root. The first snapshot is used 
        /// as the baseline and after that any new snapshot is saved as latest, which is the 
        /// one used by the GetDiff method.
        /// </summary>
        public AssetDiffer TakeSnapshot() {
            var snapshot = _assets.Refresh().ToArray();
            if (_baseLine == null) {
                _baseLine = _latest = snapshot;
            } else {
                _latest = snapshot;
            }
            return this;
        }

        IEnumerable<MediaData> _baseLine;
        IEnumerable<MediaData> _latest;

        readonly MediaDataAssets _assets;

        class MediaDataComparer : IEqualityComparer<MediaData> {
            public bool Equals(MediaData x, MediaData y) {
                return x.ContentGuid.Equals(y.ContentGuid);
            }

            public int GetHashCode(MediaData obj) {
                return obj.ContentGuid.GetHashCode();
            }
        }
    }
}