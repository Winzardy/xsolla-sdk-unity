namespace Xsolla.SDK.Store
{
    internal static class XsollaStoreClientFactory
    {
        public static IXsollaStoreClient Create()
        {
        #if UNITY_IOS && !UNITY_EDITOR
            return new XsollaStoreClientImplIOS();
        #elif UNITY_ANDROID && !UNITY_EDITOR
            return new XsollaStoreClientImplAndroid();
        #elif UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR //&& DISABLED_TMP
            return new XsollaStoreClientImplStandalone();
        #else
            return new XsollaStoreClientImplFake();
        #endif
        }
    }
}