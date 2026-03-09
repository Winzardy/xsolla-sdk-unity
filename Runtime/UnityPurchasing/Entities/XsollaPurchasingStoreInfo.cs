using System;

namespace Xsolla.SDK.Common
{
    /// <summary>
    /// Provides store information utilities for Xsolla Unity Purchasing.
    /// </summary>
    public static class XsollaPurchasingStoreInfo
    {
        /// <summary>
        /// Gets the Apple Storefront identifier.
        /// </summary>
        /// <param name="onSuccess">Callback invoked with storefront identifier on success.</param>
        /// <param name="onError">Callback invoked with error message on failure.</param>
        public static void GetAppleStorefront(Action<string> onSuccess, Action<string> onError) =>
            XsollaStoreClientInfo.GetAppleStorefront(onSuccess, onError);

        /// <summary>
        /// Gets the Apple Storefront identifier.
        /// </summary>
        /// <param name="completionHandler">
        /// Callback invoked with storefront identifier and error message.
        /// Parameters: storefront identifier, error message.
        /// </param>
        public static void GetAppleStorefront(Action<string, string> completionHandler) =>
            XsollaStoreClientInfo.GetAppleStorefront(completionHandler);
        
        /// <summary>
        /// Gets the Apple Distribution status of whether the app is running in alternative distribution.
        /// </summary>
        /// <param name="onSuccess">Callback invoked with whether the app is running in alternative distribution on success.</param>
        /// <param name="onError">Callback invoked with error message on failure.</param>
        public static void GetAppleDistributionStatus(Action<bool> onSuccess, Action<string> onError) =>
            XsollaStoreClientInfo.GetAppleDistributionStatus(onSuccess, onError);

        /// <summary>
        /// Gets the Apple Distribution status of whether the app is running in alternative distribution.
        /// </summary>
        /// <param name="completionHandler">
        /// Callback invoked with whether the app is running in alternative distribution and error message.
        /// Parameters: whether the app is running in alternative distribution, error message.
        /// </param>
        public static void GetAppleDistributionStatus(Action<bool, string> completionHandler) =>
            XsollaStoreClientInfo.GetAppleDistributionStatus(completionHandler);
    }
}