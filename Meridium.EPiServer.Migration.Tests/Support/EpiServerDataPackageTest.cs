using Meridium.EPiServer.Migration.Support;
using NFluent;
using Xunit;


namespace Meridium.EPiServer.Migration.Tests.Support {
    public class EpiServerDataPackageTest {
        public class Name_property {
            [Fact]
            public void should_return_the_name_part_of_the_package_full_path() {
                var path    = @"q:\foo\bar\baz.episerverdata";
                var package = new EpiServerDataPackage(path);

                var packageName = package.Name;

                Check.That(packageName).Equals("baz");
            }        
        }
    }
}
