using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Meridium.EPiServer.Migration.Support {
    public class PackageResolver {
        public PackageResolver(string basePath) {
            this.basePath = basePath;
            this.baseDirectory = new DirectoryInfo(this.basePath);
        }

        public EpiServerDataPackage[] GetPackages() {
            var packages = baseDirectory
                .GetFiles("*.episerverdata")
                .Select( f => new EpiServerDataPackage(f.FullName));

            return packages.ToArray();
        }

        private readonly string basePath;
        private readonly DirectoryInfo baseDirectory;
    }

    public struct EpiServerDataPackage {
        public EpiServerDataPackage(string fullPath) {
            FullPath = fullPath;
        }

        public string Name { 
            get { return PackagePattern.Match(FullPath).Groups["name"].Value; } 
        }

        public readonly string FullPath;
        private static readonly Regex PackagePattern =
            new Regex(@"^(?<path>.+?)(?<name>[^\\]+)\.episerverdata$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
    } 
}
