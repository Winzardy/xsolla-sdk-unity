#if !XSOLLA_SDK_UNITY_PURCHASING_DISABLE
using System;
using JetBrains.Annotations;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Xsolla.SDK.Store;

namespace Xsolla.SDK.UnityPurchasing
{
    /// <summary>
    /// Provides extension methods for Xsolla Unity Purchasing store.
    /// </summary>
    public interface IXsollaPurchasingStoreExtension : IStoreExtension
    {
        /// <summary>
        /// Restores previous transactions.
        /// </summary>
        /// <param name="onSuccess">Callback invoked with true if restoration succeeded.</param>
        /// <param name="onError">Callback invoked with error message if restoration failed.</param>
        void RestoreTransactions(Action<bool> onSuccess, Action<string> onError);

        /// <summary>
        /// Attempts to get the product icon URL. 
        /// </summary>
        /// <param name="product">Unity Purchasing product.</param>
        /// <param name="url">Output parameter for icon URL.</param>
        /// <returns>True if icon URL was found, otherwise false.</returns>
        bool TryGetProductIconUrl(Product product, out string url);
        
       /// <summary>
        /// Attempts to get the Xsolla store product data for a Unity Purchasing product.
        /// </summary>
        /// <param name="product">Unity Purchasing product.</param>
        /// <param name="productData">Output parameter for Xsolla store product data.</param>
        /// <returns>True if product data was found, otherwise false.</returns>
        bool TryGetProduct(Product product, out XsollaStoreClientProduct productData);

        /// <summary>
        /// Sets custom callbacks for purchase flow events.
        /// </summary>
        /// <param name="onPurchaseSucceeded">
        /// Callback invoked on successful purchase.
        /// Parameters: store callback, product ID, transaction ID, receipt.
        /// </param>
        /// <param name="onPurchaseFailed">
        /// Callback invoked on purchase failure.
        /// Parameters: store callback, purchase failure description.
        /// </param>
        void SetCustomPurchaseFlowCallbacks(
            [CanBeNull] Action<IStoreCallback, string, string, string> onPurchaseSucceeded,
            [CanBeNull] Action<IStoreCallback, PurchaseFailureDescription> onPurchaseFailed
        );

        /// <summary>
        /// Gets the validator for Xsolla purchases.
        /// </summary>
        /// <returns>Validator instance.</returns>
        XsollaPurchasingStoreValidator GetValidator();

        /// <summary>
        /// Gets the access token for the store.
        /// </summary>
        /// <param name="onSuccess">Callback invoked with access token on success.</param>
        /// <param name="onError">Callback invoked with error message on failure.</param>
        void GetAccessToken(Action<string> onSuccess, Action<string> onError);

        /// <summary>
        /// Gets the Apple Storefront identifier.
        /// </summary>
        /// <param name="onSuccess">Callback invoked with storefront identifier on success.</param>
        /// <param name="onError">Callback invoked with error message on failure.</param>
        void GetAppleStorefront(Action<string> onSuccess, Action<string> onError);

        /// <summary>
        /// Initiates a purchase for a Unity Purchasing product.
        /// </summary>
        /// <param name="product">Unity Purchasing product.</param>
        /// <param name="args">Purchase arguments.</param>
        void InitiatePurchase(Product product, XsollaStoreClientPurchaseArgs args);

        /// <summary>
        /// Initiates a purchase for a product by ID.
        /// </summary>
        /// <param name="productId">Product ID (SKU).</param>
        /// <param name="args">Purchase arguments.</param>
        void InitiatePurchase(string productId, XsollaStoreClientPurchaseArgs args);
        
        /// <summary>
        /// Updates the access token for the store.
        /// </summary>
        /// <param name="token">The new access token.</param>
        /// <param name="onSuccess">Callback invoked on successful token update.</param>
        /// <param name="onError">Callback invoked with error message on failure.</param>
        void UpdateAccessToken(string token, Action onSuccess, Action<string> onError);
    }
}
#endif