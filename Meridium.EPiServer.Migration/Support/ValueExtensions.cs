namespace Meridium.EPiServer.Migration.Support {
    public static class ValueExtensions {
        public static TValue Default<TValue>(this TValue self, TValue @default = default (TValue)) where TValue : class{
            return self ?? @default;
        } 
    }
}