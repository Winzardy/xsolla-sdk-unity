using System;
using UnityEngine;
using Xsolla.Core;

namespace Xsolla.Auth
{
	internal class WidgetAuthenticatorFactory
	{
		public IWidgetAuthenticator Create(XsollaSettings settings, Action onSuccess, Action<Error> onError, Action onCancel, string locale, SdkType sdkType)
		{
			if (Application.platform == RuntimePlatform.WebGLPlayer)
				return new WebglWidgetAuthenticator(settings, onSuccess, onError, onCancel, locale);
			
			return new StandaloneWidgetAuthenticator(settings, onSuccess, onError, onCancel, locale, sdkType);
		}
	}
}