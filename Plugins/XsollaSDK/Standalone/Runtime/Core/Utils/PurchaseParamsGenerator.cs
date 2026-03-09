using System.Collections.Generic;
using UnityEngine;

namespace Xsolla.Core
{
	internal static class PurchaseParamsGenerator
	{
		public static PurchaseParamsRequest GeneratePurchaseParamsRequest(XsollaSettings settings, PurchaseParams purchaseParams)
		{
			var purchaseSettings = new PurchaseParamsRequest.Settings {
				ui = PayStationUISettings.GenerateSettings(settings),
				redirect_policy = RedirectPolicySettings.GeneratePolicy(settings),
				external_id = purchaseParams?.external_id,
				payment_method = purchaseParams?.payment_method,
				solution = purchaseParams?.use_buy_button_solution == true ? "buy_button" : null
			};

			ProcessUiCloseButton(purchaseSettings.ui, purchaseParams);
			ProcessGooglePayQuickButton(purchaseSettings, purchaseParams);

			if (purchaseParams == null || !purchaseParams.disable_sdk_parameter)
				ProcessSdkTokenSettings(purchaseSettings);

			if (purchaseSettings.redirect_policy != null)
				purchaseSettings.return_url = purchaseSettings.redirect_policy.return_url;

			//Fix 'The array value is found, but an object is required' in case of empty values.
			if (purchaseSettings.ui == null && purchaseSettings.redirect_policy == null && purchaseSettings.return_url == null)
				purchaseSettings = null;

			var result = new PurchaseParamsRequest {
				sandbox = settings.IsSandbox,
				settings = purchaseSettings,
				custom_parameters = purchaseParams?.custom_parameters,
				currency = purchaseParams?.currency,
				locale = purchaseParams?.locale,
				country = purchaseParams?.country,
				quantity = purchaseParams?.quantity,
				shipping_data = purchaseParams?.shipping_data,
				shipping_method = purchaseParams?.shipping_method
			};

			if (purchaseParams != null && !string.IsNullOrEmpty(purchaseParams.tracking_id))
			{
				result.user = new PurchaseParamsRequest.User {
					tracking_id = new PurchaseParamsRequest.TrackingId {
						value = purchaseParams.tracking_id
					}
				};
			}

			return result;
		}

		private static void ProcessUiCloseButton(PayStationUI settings, PurchaseParams purchaseParams)
		{
			if (purchaseParams == null)
				return;

			if (purchaseParams.close_button == null && string.IsNullOrEmpty(purchaseParams.close_button_icon))
				return;

			if (settings.desktop == null)
				settings.desktop = new PayStationUI.Desktop();

			if (settings.desktop.header == null)
				settings.desktop.header = new PayStationUI.Desktop.Header();

			settings.desktop.header.close_button = purchaseParams.close_button;
			settings.desktop.header.close_button_icon = purchaseParams.close_button_icon;
		}

		private static void ProcessGooglePayQuickButton(PurchaseParamsRequest.Settings settings, PurchaseParams purchaseParams)
		{
			if (purchaseParams?.google_pay_quick_payment_button != null)
				settings.ui.gp_quick_payment_button = purchaseParams.google_pay_quick_payment_button;
		}

		private static void ProcessSdkTokenSettings(PurchaseParamsRequest.Settings settings)
		{
		}

		public static List<WebRequestHeader> GeneratePaymentHeaders(XsollaSettings settings, Dictionary<string, string> customHeaders = null)
		{
			var headers = new List<WebRequestHeader> {
				WebRequestHeader.AuthHeader(settings)
			};

			if (customHeaders == null)
				return headers;

			foreach (var kvp in customHeaders)
			{
				if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
					headers.Add(new WebRequestHeader(kvp.Key, kvp.Value));
			}

			return headers;
		}
	}
}