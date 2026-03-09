using System;
using UnityEngine;

namespace Xsolla.Core
{
	internal class XsollaToken
	{
		private const string SaveKey = "XsollaSuperToken";

		/// <summary>
		/// Access token. Required for most API requests.
		/// </summary>
		public string AccessToken => Instance?.accessToken;

		/// <summary>
		/// Refresh token. Required to get a new access token.
		///	</summary>
		public string RefreshToken => Instance?.refreshToken;

		/// <summary>
		/// Access token expiration time. Seconds since the Unix epoch.
		///	</summary>
		public int ExpirationTime => Instance?.expirationTime ?? 0;

	    /// <summary>
	    /// Returns true, if the token has expired (<see cref="ExpirationTime"/> > 0).
	    /// </summary>
	    public bool IsExpired => !Exists ||
	        ExpirationTime > 0 &&
	        DateTimeOffset.FromUnixTimeSeconds(ExpirationTime) <= DateTimeOffset.Now;

		public bool Exists => Instance != null;

	    /// <summary>
	    /// Returns true, if the token has been produced from a unique device ID.
	    /// </summary>
	    public bool IsBasedOnDeviceId => Instance?.isBasedOnDeviceId ?? false;

		private TokenData Instance { get; set; }
		private readonly XsollaSettings Settings;

		public XsollaToken(XsollaSettings settings)
		{
			Settings = settings;
		}
		
		public void Create(string accessToken, bool isBasedOnDeviceId)
		{
			Instance = new TokenData {
				accessToken = accessToken,
				isBasedOnDeviceId = isBasedOnDeviceId
			};

			XDebug.Log(Settings,"XsollaToken created (access only)"
				+ $"\nAccess token: {Instance.accessToken}"
				+ $"\nIsBasedOnDeviceId: {Instance.isBasedOnDeviceId}"
			);

			SaveInstance();
		}

		public void Create(string accessToken, string refreshToken, bool isBasedOnDeviceId)
		{
			Instance = new TokenData {
				accessToken = accessToken,
				refreshToken = refreshToken,
				isBasedOnDeviceId = isBasedOnDeviceId
			};

			XDebug.Log(Settings,"XsollaToken created (access and refresh)"
				+ $"\nAccess token: {accessToken}"
				+ $"\nRefresh token: {refreshToken}"
				+ $"\nIsBasedOnDeviceId: {Instance.isBasedOnDeviceId}"
			);

			SaveInstance();
		}

		public void Create(string accessToken, string refreshToken, int expiresIn, bool isBasedOnDeviceId)
		{
			Instance = new TokenData {
				accessToken = accessToken,
				refreshToken = refreshToken,
				expirationTime = (int) DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToUnixTimeSeconds(),
				isBasedOnDeviceId = isBasedOnDeviceId
			};

			XDebug.Log(Settings,"XsollaToken created (access and refresh and expiration time)"
				+ $"\nAccess token: {accessToken}"
				+ $"\nRefresh token: {refreshToken}"
				+ $"\nExpiration time: {DateTimeOffset.FromUnixTimeSeconds(ExpirationTime).ToLocalTime()}"
				+ $"\nIsBasedOnDeviceId: {Instance.isBasedOnDeviceId}"
			);

			SaveInstance();
		}

		private string GetSaveKey()
		{
			return $"{SaveKey}-{Settings.StoreProjectId}";
		}
		
		private void SaveInstance()
		{
			var key = GetSaveKey();
			SaveInstance(key);
		}

		private void SaveInstance(string key)
		{
			if (Instance == null)
				return;

			var json = ParseUtils.ToJson(Instance);
			PlayerPrefs.SetString(key, json);
		}

		public bool TryLoadInstance()
		{
			var key = GetSaveKey();
			
			if ( TryLoadInstance(SaveKey) ) // Migration from old key
			{
				SaveInstance();
				PlayerPrefs.DeleteKey(SaveKey);
				return true;
			}
			
			return TryLoadInstance(key);
		}

		private bool TryLoadInstance(string key)
		{
			if (!PlayerPrefs.HasKey(key))
			{
				XDebug.Log(Settings, "XsollaToken not found in PlayerPrefs");
				return false;
			}

			var json = PlayerPrefs.GetString(key);
			var data = ParseUtils.FromJson<TokenData>(json);

			if (data == null || string.IsNullOrEmpty(data.accessToken))
			{
				XDebug.Log(Settings,"XsollaToken not found in PlayerPrefs");
				return false;
			}

			Instance = data;

			if (string.IsNullOrEmpty(RefreshToken))
			{
				XDebug.Log(Settings,"XsollaToken loaded (access only)"
                    + $"\nAccess token: {AccessToken}"
                    + $"\nIsBasedOnDeviceId: {Instance.isBasedOnDeviceId}"
				);
			}
			else if (ExpirationTime <= 0)
			{
				XDebug.Log(Settings,"XsollaToken loaded (access and refresh)"
					+ $"\nAccess token: {AccessToken}"
					+ $"\nRefresh token: {RefreshToken}"
					+ $"\nIsBasedOnDeviceId: {Instance.isBasedOnDeviceId}"
				);
			}
			else
			{
				XDebug.Log(Settings,"XsollaToken loaded (access and refresh and expiration time)"
					+ $"\nAccess token: {AccessToken}"
					+ $"\nRefresh token: {RefreshToken}"
					+ $"\nExpiration time: {DateTimeOffset.FromUnixTimeSeconds(ExpirationTime).ToLocalTime()}"
					+ $"\nIsBasedOnDeviceId: {Instance.isBasedOnDeviceId}"
				);
			}

			return true;
		}

		public void DeleteSavedInstance()
		{
			var key = GetSaveKey();
			DeleteSavedInstance(key);
		}
		
		private void DeleteSavedInstance(string key)
		{
			Instance = null;
			PlayerPrefs.DeleteKey(key);
		}
	}
}
