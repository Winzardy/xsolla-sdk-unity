namespace Xsolla.SDK.Login
{
    internal static class XsollaLoginClientFactory
    {
        public static IXsollaLoginClient Create()
        {
        #if UNITY_IOS && !UNITY_EDITOR
            return new XsollaLoginClientImplIOS();
        #elif UNITY_ANDROID && !UNITY_EDITOR
            return new XsollaLoginClientImplAndroid();
        #elif UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR //&& DISABLED_TMP
            return new XsollaLoginClientImplStandalone();
        #else
            return new XsollaLoginClientImplFake();
        #endif
        }
    }
}