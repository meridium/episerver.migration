using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Meridium.EPiServer.Migration.Support;
using Xunit;
using FluentAssertions;

namespace Meridium.EPiServer.Migration.Tests.Support {
    public class PackageResolverTest {
        public class GetPackages_method {
            [Fact]
            public void should_get_a_list_of_the_packages_in_the_base_directory() {
                var basePath = TestDirectory.Get("TestAssets");
                var resolver = new PackageResolver(basePath);

                var packages = resolver.GetPackages();

                packages
                    .Select(p => p.Name)
                    .Should()
                    .BeEquivalentTo(new[] { "Foo", "Bar", "Baz" });
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