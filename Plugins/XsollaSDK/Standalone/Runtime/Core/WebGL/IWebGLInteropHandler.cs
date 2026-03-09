namespace Xsolla.Core
{
    internal interface IWebGLInteropHandler
    {
        string GetUserAgent_();
        string GetBrowserLanguage_();
        
        void OpenPayStationWidget_(string token, bool sandbox, string sdkType, string engineVersion, string sdkVersion, string applePayMerchantDomain, string appearanceJson, bool externalBrowser, string externalUrl);
        void ShowPopupAndOpenPayStation_(string url, string popupMessage, string continueButtonText, string cancelButtonText);
        void ClosePayStationWidget_();
        
        string OpenXsollaLoginWidgetPopup_(string projectId, string locale);
        string OpenXsollaLoginWidgetPopupWithConfirmation_(string projectId, string locale, string popupMessageText, string continueButtonText, string cancelButtonText);
    }
}