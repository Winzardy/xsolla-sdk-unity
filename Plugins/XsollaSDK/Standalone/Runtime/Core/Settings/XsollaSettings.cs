using System;
using UnityEngine;

namespace Xsolla.Core
{
	internal class XsollaSettings
	{
		private string loginId = string.Empty;
		private string callbackUrl;
		private bool invalidateExistingSessions;
		private int oauthClientId;

		private string storeProjectId = string.Empty;
		private bool isSandbox = true;
		private bool inAppBrowserEnabled = true;
		private bool packInAppBrowserInBuild = true;
		private string applePayMerchantDomain = string.Empty;
		private bool eventApiEnabled = false;
		private bool externalBrowserEnabled = false;

		private RedirectPolicySettings desktopRedirectPolicySettings = new RedirectPolicySettings();
		private RedirectPolicySettings webglRedirectPolicySettings = new RedirectPolicySettings();
		private RedirectPolicySettings androidRedirectPolicySettings = new RedirectPolicySettings();
		private RedirectPolicySettings iosRedirectPolicySettings = new RedirectPolicySettings();

		private PayStationUISettings desktopPayStationUISettings = new PayStationUISettings();
		private PayStationUISettings webglPayStationUISettings = new PayStationUISettings();
		private PayStationUISettings androidPayStationUISettings = new PayStationUISettings();
		private PayStationUISettings iosPayStationUISettings = new PayStationUISettings();

		// [SerializeField] private string facebookAppId;
		// [SerializeField] private string facebookClientToken;
		private string googleServerId;
		private string wechatAppId;
		private string qqAppId;

		private LogLevel logLevel = LogLevel.InfoWarningsErrors;
		private string logTag = null;
		
		private string customPayStationDomainProduction = string.Empty;
		private string customPayStationDomainSandbox = string.Empty;
		
		private XsollaToken xsollaToken;

		public bool PayStationGroupFoldout { get; set; }
		public bool RedirectPolicyGroupFoldout { get; set; }
		public bool AdvancedGroupFoldout { get; set; }

		public int PaystationVersion { get; set; } = 4;

		public XsollaSettings()
		{
			xsollaToken = new XsollaToken(this);
		}
		
		public string LoginId
		{
			get => loginId;
			set => loginId = value;
		}

		public bool InvalidateExistingSessions
		{
			get => invalidateExistingSessions;
			set => invalidateExistingSessions = value;
		}

		public int OAuthClientId
		{
			get => oauthClientId;
			set => oauthClientId = value;
		}

		public string CallbackUrl
		{
			get => callbackUrl;
			set => callbackUrl = value;
		}

		public string StoreProjectId
		{
			get => storeProjectId;
			set => storeProjectId = value;
		}

		public bool IsSandbox
		{
			get => isSandbox;
			set => isSandbox = value;
		}

		public bool InAppBrowserEnabled
		{
			get => inAppBrowserEnabled;
			set => inAppBrowserEnabled = value;
		}
		
		public bool EventApiEnabled
		{
			get => eventApiEnabled;
			set => eventApiEnabled = value;
		}

		public bool PackInAppBrowserInBuild
		{
			get => packInAppBrowserInBuild;
			set => packInAppBrowserInBuild = value;
		}

		public string ApplePayMerchantDomain
		{
			get => applePayMerchantDomain;
			set => applePayMerchantDomain = value;
		}

		public RedirectPolicySettings DesktopRedirectPolicySettings
		{
			get => desktopRedirectPolicySettings;
			set => desktopRedirectPolicySettings = value;
		}

		public RedirectPolicySettings WebglRedirectPolicySettings
		{
			get => webglRedirectPolicySettings;
			set => webglRedirectPolicySettings = value;
		}

		public RedirectPolicySettings AndroidRedirectPolicySettings
		{
			get => androidRedirectPolicySettings;
			set => androidRedirectPolicySettings = value;
		}

		public RedirectPolicySettings IosRedirectPolicySettings
		{
			get => iosRedirectPolicySettings;
			set => iosRedirectPolicySettings = value;
		}

		public PayStationUISettings DesktopPayStationUISettings
		{
			get => desktopPayStationUISettings;
			set => desktopPayStationUISettings = value;
		}

		public PayStationUISettings WebglPayStationUISettings
		{
			get => webglPayStationUISettings;
			set => webglPayStationUISettings = value;
		}

		public PayStationUISettings AndroidPayStationUISettings
		{
			get => androidPayStationUISettings;
			set => androidPayStationUISettings = value;
		}

		public PayStationUISettings IosPayStationUISettings
		{
			get => iosPayStationUISettings;
			set => iosPayStationUISettings = value;
		}

		public string FacebookAppId => string.Empty;
		public string FacebookClientToken => string.Empty;

		public string GoogleServerId
		{
			get => googleServerId;
			set => googleServerId = value;
		}

		public string WeChatAppId
		{
			get => wechatAppId;
			set => wechatAppId = value;
		}

		public string QqAppId
		{
			get => qqAppId;
			set => qqAppId = value;
		}

		public LogLevel LogLevel
		{
			get => logLevel;
			set => logLevel = value;
		}
		
		public string LogTag
		{
			get => logTag;
			set => logTag = value;
		}
		
		public string CustomPayStationDomainProduction
		{
			get => customPayStationDomainProduction;
			set => customPayStationDomainProduction = value;
		}
		
		public string CustomPayStationDomainSandbox
		{
			get => customPayStationDomainSandbox;
			set => customPayStationDomainSandbox = value;
		}
		
		public bool ExternalBrowserEnabled
		{
			get => externalBrowserEnabled;
			set => externalBrowserEnabled = value;
		}
		
		public XsollaToken XsollaToken => xsollaToken;
	}
}