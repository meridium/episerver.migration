using System;
using Meridium.EPiServer.Migration.Support;
using NFluent;
using Xunit;

namespace Meridium.EPiServer.Migration.Tests.Support {
    public class MigrationHookTest {
        public class Invoke_method {
            [Fact]
            public void should_invoke_the_callbacks_registered_for_the_specified_event() {
                MigrationHook.Clear();
                var marker = "";
                Action<BeforeImportEvent> addX = _ => marker += "x";

                MigrationHook.RegisterFor<BeforeImportEvent>(
                    addX, addX, addX
                );

                var evt = new BeforeImportEvent();
                MigrationHook.Invoke(evt);

                Check.That(marker).IsEqualTo("xxx");
            }
        }
    }
}
