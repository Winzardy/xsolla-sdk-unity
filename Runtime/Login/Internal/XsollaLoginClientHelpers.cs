using System;
using Xsolla.SDK.Common;

namespace Xsolla.SDK.Login
{
    internal class XsollaLoginClientHelpers
    {
        public const string Tag = "XsollaStoreClientHelpers";
        
        public static XsollaLoginToken JsonToToken(string json)
        {
            return XsollaClientHelpers.FromJson<XsollaLoginToken>(json);
        }
        
        public static XsollaLoginClientWebViewDismissUrl JsonToWebViewDismissUrl(string json)
        {
            return XsollaClientHelpers.FromJson<XsollaLoginClientWebViewDismissUrl>(json);
        }
        
        public static XsollaLoginToken JsonToLoginTokenInfo(string json)
        {
            var token = XsollaClientHelpers.FromJson<XsollaLoginClientTokenInfo>(json);
            var tsExpires = new DateTimeOffset(DateTime.Now.AddSeconds((double)token.expiresIn)).ToUnixTimeSeconds();
            return new XsollaLoginToken(token.accessToken, token.refreshToken, tsExpires);
        }
    }
}