using System;
using Xsolla.Auth;

namespace Xsolla.Core
{
	internal static class TokenAutoRefresher
	{
		public static void Check(XsollaSettings settings, Error error, Action<Error> onError, Action onSuccess)
		{
			if (error.ErrorType != ErrorType.InvalidToken && error.ErrorType != ErrorType.Unauthorized)
			{
				onError?.Invoke(error);
				return;
			}

			XDebug.Log($"Token is invalid. Trying to refresh token ({settings.OAuthClientId > 0}, {settings.XsollaToken.IsBasedOnDeviceId}");

			if (settings.OAuthClientId > 0) {
				XsollaAuth.RefreshToken(settings, onSuccess, onError);
			} else if (settings.XsollaToken.IsBasedOnDeviceId) {
				XsollaAuth.AuthViaDeviceID(settings, onSuccess, onError);
			} else {
				onError?.Invoke(new Error(ErrorType.InvalidToken,
					"Failed to refresh the existing token (token's origin is " +
					"unknown, cannot used authentication by device ID)"
				));
			}
		}
	}
}