using System;
using System.Collections.Generic;
using System.Linq;
using CallbackRegistry = 
    System.Collections.Generic.Dictionary<
        System.Type, 
        System.Collections.Generic.List<object>>;

namespace Meridium.EPiServer.Migration.Support {
    public static class MigrationHook {

        public static void RegisterFor<TEvent>(params Action<TEvent>[] callbacks) where TEvent : IMigrationEvent {
            var callbackList = GetCallbackList(typeof(TEvent));

            callbackList.AddRange(callbacks);
        }

        internal static void Invoke<TEvent>(TEvent evt) where TEvent : IMigrationEvent {
            var callbacks = GetCallbackList(typeof (TEvent)).Cast<Action<TEvent>>();
            foreach (var callback in callbacks) {
                callback(evt);
            }
        }

        internal static void Clear() { callbackRegistry.Clear(); }

        private static List<object> GetCallbackList(Type type) {
            List<object> list;
            if (callbackRegistry.TryGetValue(type, out list)) {
                return list;
            }

            list = new List<object>();
            callbackRegistry[type] = list;
            return list;
        } 

        private static readonly CallbackRegistry callbackRegistry =  new CallbackRegistry();
    }

    public interface IMigrationEvent {}
    public class BeforeImportEvent : IMigrationEvent {} 
}