using System;
using Meridium.EPiServer.Migration.Support;
using NFluent;
using Xunit;

namespace Meridium.EPiServer.Migration.Tests.Support {
    public class MigrationHookTest {
        public class Invoke_method {

            class TestEvent : IMigrationEvent { }

            [Fact]
            public void should_invoke_the_callbacks_registered_for_the_specified_event() {
                MigrationHook.Clear();
                var marker = "";
                Action<TestEvent> addX = _ => marker += "x";

                MigrationHook.RegisterFor<TestEvent>( 
                    addX, addX, addX 
                );

                var evt = new TestEvent();
                MigrationHook.Invoke(evt);

                Check.That(marker).IsEqualTo("xxx");
            }

            [Fact]
            public void should_just_silently_continue_when_no_callbacks_are_registered() {
                MigrationHook.Clear();
                var evt = new TestEvent();
                Check.ThatCode( () => MigrationHook.Invoke(evt) ).DoesNotThrow();
            }
        }
    }
}
