using System.IO;
using System.Linq;
using Meridium.EPiServer.Migration.Support;
using Xunit;
using NFluent;

namespace Meridium.EPiServer.Migration.Tests.Support {
    public class PackageResolverTest {
        public class GetPackages_method {
            [Fact]
            public void should_get_a_list_of_the_packages_in_the_base_directory() {
                var basePath = TestDirectory.Get("TestAssets");
                var resolver = new PackageResolver(basePath);

                var packages = resolver.GetPackages();

                Check.That(packages.Extracting("Name"))
                    .HasSize( 3 )
                    .And.Contains("Foo", "Bar", "Baz");
            }
        }
    }

    static class TestDirectory {
        public static string Get(string path) {
            var basePath = Directory.GetCurrentDirectory();
            return Path.Combine(basePath, path);
        }
    }
}