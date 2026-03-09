using UnityEngine;

namespace Xsolla.Core
{
	internal static class RedirectUrlHelper
	{
		public static string GetRedirectUrl(XsollaSettings settings, string redirectUri)
		{
			if (!string.IsNullOrEmpty(redirectUri))
				return redirectUri;

			return !string.IsNullOrEmpty(settings.CallbackUrl)
				? settings.CallbackUrl
				: Constants.DEFAULT_REDIRECT_URL;
		}

		public static string GetAuthDeepLinkUrl(XsollaSettings settings)
		{
			return string.IsNullOrEmpty(settings.CallbackUrl)
				? $"app://xlogin.{Application.identifier}"
				: settings.CallbackUrl;
		}

		public static string GetPaymentDeepLinkUrl(RedirectPolicySettings redirectPolicy)
		{
			var defaultUrl = $"app://xpayment.{Application.identifier}";

			if (redirectPolicy.UseSettingsFromPublisherAccount)
				return defaultUrl;

			var customUrl = redirectPolicy.ReturnUrl;
			return string.IsNullOrEmpty(customUrl)
				? defaultUrl
				: customUrl;
		}
	}
}