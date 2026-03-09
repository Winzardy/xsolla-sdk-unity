#if UNITY_WEBGL

using System.Runtime.InteropServices;
using UnityEngine;
using Xsolla.Core;

namespace Xsolla.Core.WebGL
{
    internal static class WebGLProviderBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            WebGLInteropHProvider.Register(new WebGLProviderBootstrapHandler());
        }

        private class WebGLProviderBootstrapHandler : IWebGLInteropHandler
        {
            [DllImport("__Internal")]
            private static extern string GetUserAgent();
            public string GetUserAgent_() => GetUserAgent();

            [DllImport("__Internal")]
            public static extern string GetBrowserLanguage();
            public string GetBrowserLanguage_() => GetBrowserLanguage();
            
            [DllImport("__Internal")]
            private static extern void OpenPayStationWidget(string token, bool sandbox, string sdkType, string engineVersion, string sdkVersion, string applePayMerchantDomain, string appearanceJson, bool externalBrowser, string externalUrl);
            public void OpenPayStationWidget_(string token, bool sandbox, string sdkType, string engineVersion, string sdkVersion, string applePayMerchantDomain, string appearanceJson, bool externalBrowser, string externalUrl)
                => OpenPayStationWidget(token, sandbox, sdkType, engineVersion, sdkVersion, applePayMerchantDomain, appearanceJson, externalBrowser, externalUrl);
            
            [DllImport("__Internal")]
            private static extern void ShowPopupAndOpenPayStation(string url, string popupMessage, string continueButtonText, string cancelButtonText);
            public void ShowPopupAndOpenPayStation_(string url, string popupMessage, string continueButtonText, string cancelButtonText)
                => ShowPopupAndOpenPayStation(url, popupMessage, continueButtonText, cancelButtonText);
            
            [DllImport("__Internal")]
            private static extern void ClosePayStationWidget();
            public void ClosePayStationWidget_() => ClosePayStationWidget();
            
            [DllImport("__Internal")]
            private static extern string OpenXsollaLoginWidgetPopup(string projectId, string locale);
            public string OpenXsollaLoginWidgetPopup_(string projectId, string locale) 
                => OpenXsollaLoginWidgetPopup(projectId, locale);

            [DllImport("__Internal")]
            private static extern string OpenXsollaLoginWidgetPopupWithConfirmation(string projectId, string locale, string popupMessageText, string continueButtonText, string cancelButtonText);
            public string OpenXsollaLoginWidgetPopupWithConfirmation_(string projectId, string locale, string popupMessageText, string continueButtonText, string cancelButtonText) 
                => OpenXsollaLoginWidgetPopupWithConfirmation(projectId, locale, popupMessageText, continueButtonText, cancelButtonText);
        }
    }
}
#endif