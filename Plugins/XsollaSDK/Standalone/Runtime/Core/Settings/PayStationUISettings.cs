using System;
using UnityEngine;

namespace Xsolla.Core
{
	[Serializable]
	internal class PayStationUISettings
	{
		public bool isFoldout = true;

		public string paystationThemeId = "63295aab2e47fab76f7708e3";
		public string size;
		public bool? showCloseButton;
		public bool? visibleLogo;

		public static PayStationUI GenerateSettings(XsollaSettings settings)
		{
			var platfromSpecificSettings = GePlatformSpecificSettings(settings);
			var paymentSettings = new PayStationUI {
				theme = platfromSpecificSettings.paystationThemeId,
				size = platfromSpecificSettings.size,
			};

			if (Application.platform == RuntimePlatform.WindowsPlayer ||
			    Application.platform == RuntimePlatform.OSXPlayer ||
			    Application.isEditor)
			{

				if (settings.InAppBrowserEnabled)
					paymentSettings.is_independent_windows = true;

				if (platfromSpecificSettings.showCloseButton != null)
				{
					if (paymentSettings.desktop == null)
						paymentSettings.desktop = new PayStationUI.Desktop();
					if (paymentSettings.desktop.header == null)
						paymentSettings.desktop.header = new PayStationUI.Desktop.Header();

					paymentSettings.desktop.header.close_button = platfromSpecificSettings.showCloseButton;
				}
			}
			
			if (platfromSpecificSettings.visibleLogo != null)
			{
				if (paymentSettings.desktop == null)
					paymentSettings.desktop = new PayStationUI.Desktop();
				if (paymentSettings.desktop.header == null)
					paymentSettings.desktop.header = new PayStationUI.Desktop.Header();
				
				paymentSettings.desktop.header.visible_logo = platfromSpecificSettings.visibleLogo;
			}
			

			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				if (!string.IsNullOrEmpty(settings.ApplePayMerchantDomain))
					paymentSettings.is_independent_windows = null;
			}

			return paymentSettings;
		}

		private static PayStationUISettings GePlatformSpecificSettings(XsollaSettings settings)
		{
			if (Application.platform == RuntimePlatform.WebGLPlayer)
				return settings.WebglPayStationUISettings;

			return settings.DesktopPayStationUISettings;
		}
	}
}