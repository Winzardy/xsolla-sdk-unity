using Xsolla.SDK.Common;

namespace Xsolla.SDK.Login
{
    internal interface IXsollaLoginClient
    {
        void Login(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError);
        void LoginSilently(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError);
        void LoginWithSocialAccount(XsollaClientConfiguration configuration, string provider, string accountToken, LoginResultFunc onSuccess, ErrorFunc onError);
        
        void ClearToken(XsollaClientConfiguration configuration, ClearTokenResultFunc onSuccess, ErrorFunc onError);
        void RefreshToken(XsollaClientConfiguration configuration, XsollaLoginToken token, LoginResultFunc onSuccess, ErrorFunc onError);
    }
}