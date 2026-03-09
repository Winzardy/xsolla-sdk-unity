using UnityEngine;

namespace Xsolla.Core
{
	internal static class WebHelper
	{
		private static string GetUserAgent()
		{
			if (WebGLInteropHProvider.Handler != null)
				return WebGLInteropHProvider.Handler.GetUserAgent_();
			
			XDebug.LogError("WebGLInteropHProvider not found: GetUserAgent");
			return "";
		}

		public static string GetBrowserLanguage()
		{
			if (WebGLInteropHProvider.Handler != null)
				return WebGLInteropHProvider.Handler.GetBrowserLanguage_();
			
			XDebug.LogError("WebGLInteropHProvider not found: GetBrowserLanguage");
			return "";
		}

		public static bool IsBrowserSafari()
		{
			var userAgent = GetUserAgent();

			if (Application.isMobilePlatform)
			{
				return (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
					&& userAgent.Contains("Safari")
					&& !userAgent.Contains("CriOS");
			}

			return userAgent.Contains("Safari")
				&& userAgent.Contains("AppleWebKit")
				&& !userAgent.Contains("Chrome")
				&& !userAgent.Contains("Chromium");
		}
	}
}
