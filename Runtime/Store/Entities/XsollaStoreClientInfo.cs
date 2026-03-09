using System;
using Xsolla.SDK.Store;
using Xsolla.SDK.Utils;

namespace Xsolla.SDK.Common
{
    /// <summary>
    /// Provides methods to retrieve Apple/Google Play Storefront and Distribution information for Xsolla SDK.
    /// </summary>
    public static class XsollaStoreClientInfo
    {
        /// <summary>
        /// Country code in <b>ISO-3166-1 alpha-2</b> format (e.g., <c>"US"</c> for United States).
        /// </summary>
        public readonly struct CountryCode
        {
            public static readonly CountryCode US = new CountryCode("US");

            public readonly string code;

            public CountryCode(string countryCode)
            {
                code = countryCode;
            }

            /// <summary>
            /// Checks whether the <see cref="CountryCode"/> matches the specified raw country
            /// code string in <b>ISO-3166-1 alpha-2</b> format.
            /// </summary>
            /// <remarks>Case-insensitive.</remarks>
            public bool IsCode(string countryCode)
            {
                return code.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Gets the Apple Storefront identifier.
        /// </summary>
        /// <param name="onSuccess">Callback invoked with the storefront identifier on success.</param>
        /// <param name="onError">Callback invoked with an error message on failure.</param>
        public static void GetAppleStorefront(Action<string> onSuccess, Action<string> onError)
        {
            RunOnStartThread.Create();
            
#if UNITY_IOS
            XsollaStoreClientImplIOS.GetAppleStorefront_(
                onSuccess: storefront => onSuccess?.Invoke(storefront),
                onError: error => onError?.Invoke(error)
            );
#else
            onError?.Invoke("unsupported platform");
#endif
        }

        /// <summary>
        /// Gets the Apple Storefront identifier and error message using a completion handler.
        /// </summary>
        /// <param name="completionHandler">
        /// Callback invoked with the storefront identifier and error message.
        /// The first parameter is the storefront identifier (or null on error),
        /// the second parameter is the error message (or null on success).
        /// </param>
        public static void GetAppleStorefront(Action<string, string> completionHandler)
        {
            RunOnStartThread.Create();
            
            GetAppleStorefront(
                onSuccess: storefront => completionHandler?.Invoke(storefront, null),
                onError: error => completionHandler?.Invoke(null, error)
            );
        }
        
        /// <summary>
        /// Gets the Apple Distribution status of whether the app is running in alternative distribution.
        /// </summary>
        /// <param name="onSuccess">Callback invoked with whether the app is running in alternative distribution on success.</param>
        /// <param name="onError">Callback invoked with an error message on failure.</param>
        public static void GetAppleDistributionStatus(Action<bool> onSuccess, Action<string> onError)
        {
            RunOnStartThread.Create();
            
#if UNITY_IOS
            XsollaStoreClientImplIOS.GetAppleDistribution_(
                onSuccess: isRunningInAlternativeDistribution => onSuccess?.Invoke(isRunningInAlternativeDistribution),
                onError: error => onError?.Invoke(error)
            );
#else
            onError?.Invoke("unsupported platform");
#endif
        }
        
        /// <summary>
        /// Gets the Apple Distribution status of whether the app is running in alternative distribution and error message using a completion handler.
        /// </summary>
        /// <param name="completionHandler">
        /// Callback invoked with the distribution status and error message.
        /// The first parameter is whether the app is running in alternative distribution (or false on error),
        /// the second parameter is the error message (or null on success).
        /// </param>
        public static void GetAppleDistributionStatus(Action<bool, string> completionHandler)
        {
            RunOnStartThread.Create();
            
            GetAppleDistributionStatus(
                onSuccess: isRunningInAlternativeDistribution => completionHandler?.Invoke(isRunningInAlternativeDistribution, null),
                onError: error => completionHandler?.Invoke(false, error)
            );
        }

        /// <summary>
        /// Asynchronously queries Google Play storefront's <see cref="CountryCode"/>.
        /// </summary>
        /// <param name="onSuccess">Invoked once the country code was successfully retrieved.</param>
        /// <param name="onError">Invoked if there was an error retrieving the storefront's country code.</param>
        /// <seealso cref="IsGooglePlayStoreInstalled" />
        public static void QueryGooglePlayCountryCodeAsync(
            Action<CountryCode> onSuccess, Action<string> onError
        )
        {
            RunOnStartThread.Create();
            
#if UNITY_ANDROID
            XsollaStoreClientInfoAndroid.QueryGooglePlayCountryCodeAsync(onSuccess, onError);
#else
            onError.Invoke("Not supported on current platform");
#endif // UNITY_ANDROID
        }

        public static void QueryGooglePlayCountryCodeAsync(Action<CountryCode, string> completionHandler)
            => QueryGooglePlayCountryCodeAsync(
                onSuccess: (result) => completionHandler?.Invoke(result, null),
                onError: (error) => completionHandler?.Invoke(default, error)
            );

        public static string GetInstallerPackageName()
        {
#if UNITY_ANDROID
            return XsollaStoreClientInfoAndroid.GetInstallerPackageName();
#else
            return string.Empty;
#endif // UNITY_ANDROID
        }

        public static bool IsInstallerPackageName(string packageName)
        {
#if UNITY_ANDROID
            return XsollaStoreClientInfoAndroid.IsInstallerPackageName(packageName);
#else
            return false;
#endif // UNITY_ANDROID
        }

        /// <summary>
        /// Checks whether the Google Play Store is installed on the device.
        /// </summary>
        public static bool IsGooglePlayStoreInstalled()
        {
#if UNITY_ANDROID
            return XsollaStoreClientInfoAndroid.IsGooglePlayStoreInstalled();
#else
            return false;
#endif // UNITY_ANDROID
        }

        /// <summary>
        /// Checks whether the app has been installed from Google Play Store (or side-loaded).
        /// </summary>
        public static bool IsInstalledFromGooglePlayStore()
        {
#if UNITY_ANDROID
            return XsollaStoreClientInfoAndroid.IsInstalledFromGooglePlayStore();
#else
            return false;
#endif // UNITY_ANDROID
        }
    }
}
