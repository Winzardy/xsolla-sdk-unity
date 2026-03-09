#if !XSOLLA_SDK_UNITY_PURCHASING_DISABLE
using System;
using UnityEngine.Purchasing.Security;
using Xsolla.SDK.Store;

namespace Xsolla.SDK.UnityPurchasing
{
    /// <summary>
    /// Represents a purchase receipt for Xsolla Unity Purchasing store.
    /// </summary>
    public class XsollaPurchasingStorePurchaseReceipt : IPurchaseReceipt
    {
        /// <summary>
        /// Gets the transaction ID of the purchase.
        /// </summary>
        public string transactionID => receipt.transactionId;

        /// <summary>
        /// Gets the product ID (SKU) of the purchased product.
        /// </summary>
        public string productID => receipt.productId;

        /// <summary>
        /// Gets the order ID of the purchase.
        /// </summary>
        public int orderId => receipt.orderId;

        /// <summary>
        /// Gets the date and time of the purchase.
        /// </summary>
        public DateTime purchaseDate { get; private set; }

        private readonly XsollaStoreClientPurchaseReceipt receipt;

        /// <summary>
        /// Initializes a new instance of <see cref="XsollaPurchasingStorePurchaseReceipt"/>.
        /// </summary>
        /// <param name="receipt">The Xsolla store client purchase receipt.</param>
        public XsollaPurchasingStorePurchaseReceipt(XsollaStoreClientPurchaseReceipt receipt)
        {
            this.receipt = receipt;
            purchaseDate = DateTime.Now;
        }

        /// <summary>
        /// Determines whether the purchase status is finished.
        /// </summary>
        /// <param name="status">The status string to check.</param>
        /// <returns>True if the status is "done", otherwise false.</returns>
        public static bool IsFinished(string status)
        {
            return status.Equals("done", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif