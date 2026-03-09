using Xsolla.SDK.Common;

namespace Xsolla.SDK.Login
{
    internal class XsollaLoginClientImplFake : IXsollaLoginClient
    {
        private const string Tag = "XsollaLoginClientImplFake";
        
        public void Login(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, "Login");
            onSuccess?.Invoke(XsollaLoginToken.Empty);
        }

        public void LoginSilently(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, "LoginSilently");
            onSuccess?.Invoke(XsollaLoginToken.Empty);
        }
        
        public void LoginWithSocialAccount(XsollaClientConfiguration configuration, string provider, string accountToken, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, "LoginWithSocialAccount");
            onSuccess?.Invoke(XsollaLoginToken.Empty);
        }

        public void ClearToken(XsollaClientConfiguration configuration, ClearTokenResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, "ClearToken");
            onSuccess?.Invoke();
        }
        
        public void RefreshToken(XsollaClientConfiguration configuration, XsollaLoginToken token, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, "RefreshToken");
            onSuccess?.Invoke(token);
        }
        
        
    }
}