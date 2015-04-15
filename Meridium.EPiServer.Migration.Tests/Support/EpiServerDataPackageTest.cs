using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meridium.EPiServer.Migration.Support; 
using Xunit;
using FluentAssertions;

namespace Meridium.EPiServer.Migration.Tests.Support {
    public class EpiServerDataPackageTest {
        public class Name_property {
            [Fact]
            public void should_return_the_name_part_of_the_package_full_path() {
                var path    = @"q:\foo\bar\baz.episerverdata";
                var package = new EpiServerDataPackage(path);

                var packageName = package.Name;

                packageName.Should().Be("baz");
            }        
        }
    }
}
