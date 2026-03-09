#if UNITY_ANDROID || UNITY_EDITOR

using System;
using UnityEngine;
using Xsolla.SDK.Common;
using Xsolla.SDK.Utils;

namespace Xsolla.SDK.Store
{
    internal static class XsollaStoreClientInfoAndroid
    {
        public static void QueryGooglePlayCountryCodeAsync(
            Action<XsollaStoreClientInfo.CountryCode> onSuccess, Action<string> onError
        )
        {
            if (Application.isEditor)
            {
                onSuccess?.Invoke(XsollaStoreClientInfo.CountryCode.US);
                return;
            }

            try
            {
                var androidActivity = XsollaClientBridgeHelpersAndroid.androidActivity.Value;

                XsollaClientBridgeHelpersAndroid.JavaCallStatic("QueryGooglePlayCountryCodeAsync",
                    androidActivity.activity, new XsollaClientBridgeHelpersAndroid.XsollaUnityBridgeJsonCallback(
                        (countryCode, err) =>
                        {
                            RunOnStartThread.Run(() =>
                            {
                                if (err != null)
                                    onError(err);
                                else
                                    onSuccess(new XsollaStoreClientInfo.CountryCode(countryCode));
                            });
                        }
                    )
                );
            }
            catch (Exception e)
            {
                XsollaLogger.Error("XsollaStoreClientInfoAndroid",
                    $"Failed to query Google Play Country Code:\n{e}"
                );
                onError(e.Message);
            }
        }

        public static string GetInstallerPackageName()
        {
            if (Application.isEditor) return string.Empty;

            try
            {
                var androidActivity = XsollaClientBridgeHelpersAndroid.androidActivity.Value;
                return XsollaClientBridgeHelpersAndroid.JavaCallStatic<string>(
                    "GetInstallerPackageName", androidActivity.activity
                );
            }
            catch (Exception e)
            {
                XsollaLogger.Error("XsollaStoreClientInfoAndroid",
                    $"Failed to query installer package name:\n{e}"
                );
                return String.Empty;
            }
        }

        public static bool IsInstallerPackageName(string packageName)
        {
            if (Application.isEditor) return false;

            try
            {
                var androidActivity = XsollaClientBridgeHelpersAndroid.androidActivity.Value;
                return XsollaClientBridgeHelpersAndroid.JavaCallStatic<bool>(
                    "IsInstallerPackageName", androidActivity.activity, packageName
                );
            }
            catch (Exception e)
            {
                XsollaLogger.Error("XsollaStoreClientInfoAndroid",
                    $"Failed to verify installer package name:\n{e}"
                );
                return false;
            }
        }

        /// <summary>
        /// Checks whether the Google Play Store is installed on the device.
        /// </summary>
        public static bool IsGooglePlayStoreInstalled()
        {
            if (Application.isEditor) return false;

            try
            {
                var androidActivity = XsollaClientBridgeHelpersAndroid.androidActivity.Value;
                return XsollaClientBridgeHelpersAndroid.JavaCallStatic<bool>(
                    "IsGooglePlayStoreInstalled", androidActivity.activity
                );
            }
            catch (Exception e)
            {
                XsollaLogger.Error("XsollaStoreClientInfoAndroid",
                    $"Failed to query whether the Google Play Store app is installed:\n{e}"
                );
                return false;
            }
        }

        /// <summary>
        /// Checks whether the app has been installed from Google Play Store (or side-loaded).
        /// </summary>
        public static bool IsInstalledFromGooglePlayStore()
        {
            if (Application.isEditor) return false;

            try
            {
                var androidActivity = XsollaClientBridgeHelpersAndroid.androidActivity.Value;
                return XsollaClientBridgeHelpersAndroid.JavaCallStatic<bool>(
                    "IsInstalledFromGooglePlayStore", androidActivity.activity
                );
            }
            catch (Exception e)
            {
                XsollaLogger.Error("XsollaStoreClientInfoAndroid",
                    $"Failed to query whether the app has been installed from Google Play Store:\n{e}"
                );
                return false;
            }
        }
    }
}

#endif // UNITY_ANDROID || UNITY_EDITOR
