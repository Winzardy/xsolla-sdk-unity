using System;
using UnityEngine;

namespace Xsolla.Core
{
	internal static class XsollaWebBrowserHandlerWebGL
	{
		private static void OpenPayStationWidget(string token, bool sandbox, string sdkType,
			string engineVersion, string sdkVersion, string applePayMerchantDomain, string appearanceJson, bool externalBrowser, string externalUrl)
		{
			if (WebGLInteropHProvider.Handler != null)
			{
				WebGLInteropHProvider.Handler.OpenPayStationWidget_(token, sandbox, sdkType, engineVersion, sdkVersion, applePayMerchantDomain, appearanceJson, externalBrowser, externalUrl);
				return;
			}
			
			XDebug.LogError("WebGLInteropHProvider not found: OpenPayStationWidget");
		}

		private static void ShowPopupAndOpenPayStation(string url, string popupMessage, string continueButtonText,
			string cancelButtonText)
		{
			if (WebGLInteropHProvider.Handler != null)
			{
				WebGLInteropHProvider.Handler.ShowPopupAndOpenPayStation_(url, popupMessage, continueButtonText, cancelButtonText);
				return;
			}
			
			XDebug.LogError("WebGLInteropHProvider not found: ShowPopupAndOpenPayStation");
		}

		private static void ClosePayStationWidget()
		{
			if (WebGLInteropHProvider.Handler != null)
			{
				WebGLInteropHProvider.Handler.ClosePayStationWidget_();
				return;
			}
			
			XDebug.LogError("WebGLInteropHProvider not found: ClosePayStationWidget");
		}

		private static Action<bool> BrowserClosedCallback;

		public static void OpenPayStation(XsollaSettings settings, string token, Action<BrowserCloseInfo> onBrowserClosed, WebGlAppearance appearance, string externalUrl, SdkType sdkType)
		{
			Screen.fullScreen = false;
			OpenPayStationWidgetImmediately(settings, token, onBrowserClosed, appearance, externalUrl, sdkType);
		}

		public static void ClosePayStation(bool isManually)
		{
			BrowserClosedCallback?.Invoke(isManually);
			ClosePayStationWidget();
		}

		private static void OpenPayStationWidgetImmediately(XsollaSettings settings, string token, Action<BrowserCloseInfo> onBrowserClosed, WebGlAppearance appearance, string externalUrl, SdkType sdkType)
		{
			if (appearance == null)
				appearance = new WebGlAppearance();

			BrowserClosedCallback = isManually => {
				var info = new BrowserCloseInfo {
					isManually = isManually
				};
				onBrowserClosed?.Invoke(info);
			};

			OpenPayStationWidget(
				token,
				settings.IsSandbox,
				WebRequestHelper.GetSdkType(sdkType),
				Application.unityVersion,
				Info.SDK_VERSION,
				settings.ApplePayMerchantDomain,
				ParseUtils.ToJson(appearance),
				externalBrowser: settings.ExternalBrowserEnabled,
				externalUrl: externalUrl
			);
		}

		public static void OpenUrlInNewTab(string url)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			Application.ExternalEval($"window.open('{url}', '_blank')");
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
