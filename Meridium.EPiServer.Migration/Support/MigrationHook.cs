using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using EPiServer.Core;
using EPiServer.Enterprise;
using CallbackRegistry = 
    System.Collections.Generic.Dictionary<
        System.Type, 
        System.Collections.Generic.List<object>>;

namespace Meridium.EPiServer.Migration.Support {
    public static class MigrationHook {

        public static void RegisterFor<TEvent>(params Action<TEvent, IMigrationLog>[] callbacks) where TEvent : IMigrationEvent {
            var callbackList = GetCallbackList(typeof(TEvent));

            callbackList.AddRange(callbacks);
        }

        internal static void Invoke<TEvent>(TEvent evt, IMigrationLog log) where TEvent : IMigrationEvent {
            var callbacks = GetCallbackList(typeof (TEvent)).Cast<Action<TEvent, IMigrationLog>>();
            foreach (var callback in callbacks) {
                callback(evt, log);
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

    public class DataImporterEvent<TArgs> : IMigrationEvent {
        public DataImporterEvent(TArgs e) { ImportData = e; }
        public TArgs ImportData { get; private set; }
    } 

    public class BeforePageImportEvent : DataImporterEvent<ContentImportingEventArgs> {
        public BeforePageImportEvent(ContentImportingEventArgs args) : base(args){}
    }

    public class AfterPageImportEvent : DataImporterEvent<ContentImportedEventArgs> {
        public AfterPageImportEvent(ContentImportedEventArgs args) : base(args){}
    }

    public class BeforeFileImportEvent : DataImporterEvent<FileImportingEventArgs> {
        public BeforeFileImportEvent(FileImportingEventArgs args) : base(args){}
    }

    public class AfterFileImportEvent : DataImporterEvent<FileImportedEventArgs> {
        public AfterFileImportEvent(FileImportedEventArgs args) : base(args){}
    }

    public class BeforePageTransformEvent : IMigrationEvent {
        public BeforePageTransformEvent(PageData page) { Page = page; }
        public PageData Page { get; private set; }
    }

    public class AfterPageTransformEvent : IMigrationEvent {
        public AfterPageTransformEvent(ContentReference contentReference) {
            ContentReference = contentReference;
        }
        public ContentReference ContentReference { get; private set; }
    } 
}