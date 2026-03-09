#if UNITY_IOS || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using Xsolla.SDK.Common;
using Xsolla.SDK.Utils;

namespace Xsolla.SDK.Login
{
    internal class XsollaLoginClientImplIOS : IXsollaLoginClient
    {
        private const string Tag = "XsollaLoginClientImplIOS";
        
        [DllImport("__Internal")]
        private static extern void _XsollaUnityBridgeLoginWidget(string configJson, XsollaClientBridgeHelpersIOS.XsollaUnityBridgeJsonCallbackDelegate callback, Int64 callbackData);
        public void Login(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, $"Login: {configuration}");
            
            RunOnStartThread.Create();
            
            _XsollaUnityBridgeLoginWidget(
                configJson: XsollaClientHelpers.ConfigurationToJson(configuration),
                callback: XsollaClientBridgeHelpersIOS.OnXsollaUnityBridgeJsonCallback,
                callbackData: XsollaClientBridgeHelpersIOS.CreateCallbackData( "Login",
                    onSuccess: json => onSuccess?.Invoke(XsollaLoginClientHelpers.JsonToLoginTokenInfo(json)),
                    onError: error => onError?.Invoke(error)
                )
            );
        }
        
        [DllImport("__Internal")]
        private static extern void _XsollaUnityBridgeLoadWidgetAuthToken(string configJson, XsollaClientBridgeHelpersIOS.XsollaUnityBridgeJsonCallbackDelegate callback, Int64 callbackData);
        public void LoginSilently(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, $"LoginSilently: {configuration}");
            
            RunOnStartThread.Create();
            
            _XsollaUnityBridgeLoadWidgetAuthToken(
                configJson: XsollaClientHelpers.ConfigurationToJson(configuration),
                callback: XsollaClientBridgeHelpersIOS.OnXsollaUnityBridgeJsonCallback,
                callbackData: XsollaClientBridgeHelpersIOS.CreateCallbackData( "LoginSilently",
                    onSuccess: json => onSuccess?.Invoke(XsollaLoginClientHelpers.JsonToLoginTokenInfo(json)),
                    onError: error => onError?.Invoke(error)
                )
            );
        }
        
        [DllImport("__Internal")]
        private static extern void _XsollaUnityBridgeLoginWithSocialAccount(string configJson, string provider, string accountToken, XsollaClientBridgeHelpersIOS.XsollaUnityBridgeJsonCallbackDelegate callback, Int64 callbackData);
        
        public void LoginWithSocialAccount(XsollaClientConfiguration configuration, string provider, string accountToken, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, $"Login with social account: {configuration}");
            
            RunOnStartThread.Create();
            
            _XsollaUnityBridgeLoginWithSocialAccount(
                configJson: XsollaClientHelpers.ConfigurationToJson(configuration),
                provider: provider,
                accountToken: accountToken,
                callback: XsollaClientBridgeHelpersIOS.OnXsollaUnityBridgeJsonCallback,
                callbackData: XsollaClientBridgeHelpersIOS.CreateCallbackData( "LoginWithSocialAccount",
                    onSuccess: json => onSuccess?.Invoke(XsollaLoginClientHelpers.JsonToLoginTokenInfo(json)),
                    onError: error => onError?.Invoke(error)
                )
            );
        }

        [DllImport("__Internal")]
        private static extern void _XsollaUnityBridgeClearToken(string configJson, XsollaClientBridgeHelpersIOS.XsollaUnityBridgeJsonCallbackDelegate callback, Int64 callbackData);

        public void ClearToken(XsollaClientConfiguration configuration, ClearTokenResultFunc onSuccess,
            ErrorFunc onError)
        {
            XsollaLogger.Debug(Tag, $"ClearToken: {configuration}");
            
            RunOnStartThread.Create();
            
            _XsollaUnityBridgeClearToken(
                configJson: XsollaClientHelpers.ConfigurationToJson(configuration),
                callback: XsollaClientBridgeHelpersIOS.OnXsollaUnityBridgeJsonCallback,
                callbackData: XsollaClientBridgeHelpersIOS.CreateCallbackData( "ClearToken",
                    onSuccess: json => onSuccess?.Invoke(),
                    onError: error => onError?.Invoke(error)
                ));
        }
        
        
        [DllImport("__Internal")]
        private static extern void _XsollaUnityBridgeRefreshToken(string configJson, string accessToken, string refreshToken, int expiresIn, XsollaClientBridgeHelpersIOS.XsollaUnityBridgeJsonCallbackDelegate callback, Int64 callbackData);

        public void RefreshToken(
            XsollaClientConfiguration configuration, XsollaLoginToken token, LoginResultFunc onSuccess, ErrorFunc onError
        ){
            XsollaLogger.Debug(Tag, $"RefreshToken: {configuration}");
            
            RunOnStartThread.Create();
            
            _XsollaUnityBridgeRefreshToken(
                configJson: XsollaClientHelpers.ConfigurationToJson(configuration),
                accessToken: token.accessToken,
                refreshToken: token.refreshToken,
                expiresIn: (int)token.expiresIn, 
                callback: XsollaClientBridgeHelpersIOS.OnXsollaUnityBridgeJsonCallback,
                callbackData: XsollaClientBridgeHelpersIOS.CreateCallbackData( "RefreshToken",
                    onSuccess: json => onSuccess?.Invoke(XsollaLoginClientHelpers.JsonToLoginTokenInfo(json)),
                    onError: error => onError?.Invoke(error)
                ));
        }
        
        
        [DllImport("__Internal")]
        private static extern void _XsollaUnityBridgeGetWebViewDismissUrl(string configJson, XsollaClientBridgeHelpersIOS.XsollaUnityBridgeJsonCallbackDelegate callback, Int64 callbackData);
        
        public void GetWebViewDismissUrl(
            XsollaClientConfiguration configuration, WebViewDismissUrlFunc onSuccess, ErrorFunc onError
        ){
            XsollaLogger.Debug(Tag, $"GetWebViewDismissUrl: {configuration}");
            
            RunOnStartThread.Create();
            
            _XsollaUnityBridgeGetWebViewDismissUrl(
                configJson: XsollaClientHelpers.ConfigurationToJson(configuration),
                callback: XsollaClientBridgeHelpersIOS.OnXsollaUnityBridgeJsonCallback,
                callbackData: XsollaClientBridgeHelpersIOS.CreateCallbackData( "GetWebViewDismissUrl",
                    onSuccess: json => onSuccess?.Invoke(XsollaLoginClientHelpers.JsonToWebViewDismissUrl(json)),
                    onError: error => onError?.Invoke(error)
                ));
        }
    }
}
#endif