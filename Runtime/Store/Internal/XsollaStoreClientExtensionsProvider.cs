using Xsolla.SDK.Common.Extensions;

namespace Xsolla.SDK.Store
{
    internal interface IXsollaStoreClientExtensionsHandler
    {
        public RetryPolicies GetRetryPolicies();
    }
    
    internal static class XsollaStoreClientExtensionsProvider
    {
        public static IXsollaStoreClientExtensionsHandler Handler { get; private set; }
        public static void Register(IXsollaStoreClientExtensionsHandler handler) => Handler = handler;
        public static void Unregister(IXsollaStoreClientExtensionsHandler handler)
        {
            if (Handler == handler) Handler = null;
        }
    }
}