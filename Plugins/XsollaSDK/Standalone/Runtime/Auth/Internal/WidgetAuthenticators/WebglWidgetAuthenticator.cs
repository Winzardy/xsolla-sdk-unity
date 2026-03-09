using System;
using UnityEngine;
using Xsolla.Core;

namespace Xsolla.Auth
{
	internal class WebglWidgetAuthenticator : IWidgetAuthenticator
	{
		private static string OpenXsollaLoginWidgetPopup(string projectId, string locale)
		{
			if (WebGLInteropHProvider.Handler != null)
				return WebGLInteropHProvider.Handler.OpenXsollaLoginWidgetPopup_(projectId, locale);
			
			XDebug.LogError("WebGLInteropHProvider not found: OpenXsollaLoginWidgetPopup");
			return "";
		}

		private static string OpenXsollaLoginWidgetPopupWithConfirmation(string projectId, string locale,
			string popupMessageText, string continueButtonText, string cancelButtonText)
		{
			if (WebGLInteropHProvider.Handler != null)
				return WebGLInteropHProvider.Handler.OpenXsollaLoginWidgetPopupWithConfirmation_(projectId, locale, popupMessageText, continueButtonText, cancelButtonText);
			
			XDebug.LogError("WebGLInteropHProvider not found: OpenXsollaLoginWidgetPopupWithConfirmation");
			return "";
		}

		private readonly Action OnSuccessCallback;
		private readonly Action<Error> OnErrorCallback;
		private readonly Action OnCancelCallback;
		private readonly string Locale;
		private readonly XsollaSettings Settings;

		public WebglWidgetAuthenticator(XsollaSettings settings, Action onSuccessCallback, Action<Error> onErrorCallback, Action onCancelCallback, string locale)
		{
			OnSuccessCallback = onSuccessCallback;
			OnErrorCallback = onErrorCallback;
			OnCancelCallback = onCancelCallback;
			Locale = locale;
			Settings = settings;
		}

		public void Launch()
		{
			Screen.fullScreen = false;
			LogMessage("Launch");
			SubscribeToWebCallbacks(Settings);

			OpenImmediately(Settings);
		}

		private void OpenImmediately(XsollaSettings settings)
		{
			LogMessage("Open widget without confirmation");
			OpenXsollaLoginWidgetPopup(Settings.LoginId, Locale);
		}

		private void OnAuthSuccessWebCallbackReceived(string data)
		{
			LogMessage($"OnAuthSuccessWebCallbackReceived. Data: {data}");
			UnsubscribeFromWebCallbacks();
			Settings.XsollaToken.Create(data, isBasedOnDeviceId: false);
			OnSuccessCallback?.Invoke();
		}

		private void OnAuthCancelWebCallbackReceived()
		{
			LogMessage("OnAuthCancelWebCallbackReceived");
			UnsubscribeFromWebCallbacks();
			OnCancelCallback?.Invoke();
		}

		private void OnAuthOpenPopup(XsollaSettings settings, string projectId, string locale)
		{
			LogMessage("OnAuthOpenPopup");
			OpenImmediately(settings);
		}

		private void SubscribeToWebCallbacks(XsollaSettings settings)
		{
			XsollaWebCallbacks.Instance.WidgetAuthSuccess += OnAuthSuccessWebCallbackReceived;
			XsollaWebCallbacks.Instance.WidgetAuthCancel += OnAuthCancelWebCallbackReceived;
			XsollaWebCallbacks.Instance.WidgetOpen += OnAuthOpenPopup;
		}

		private void UnsubscribeFromWebCallbacks()
		{
			XsollaWebCallbacks.Instance.WidgetAuthSuccess -= OnAuthSuccessWebCallbackReceived;
			XsollaWebCallbacks.Instance.WidgetAuthCancel -= OnAuthCancelWebCallbackReceived;
			XsollaWebCallbacks.Instance.WidgetOpen -= OnAuthOpenPopup;
		}

		private void LogMessage(string message)
		{
			XDebug.Log("WebglWidgetAuthenticator: " + message);
		}
	}
}
