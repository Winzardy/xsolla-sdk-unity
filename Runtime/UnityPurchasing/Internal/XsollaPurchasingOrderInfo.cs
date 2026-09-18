#if !XSOLLA_SDK_UNITY_PURCHASING_DISABLE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Xsolla.SDK.UnityPurchasing
{
    /// <summary>
    /// Order details reported by the Xsolla store to Unity IAP.
    /// </summary>
    internal class XsollaPurchasingOrderInfo : IOrderInfo
    {
        /// <summary>
        /// Apple specific order details. Always null, Xsolla is not an Apple store.
        /// </summary>
        public IAppleOrderInfo Apple => null;

        /// <summary>
        /// Google specific order details. Always null, Xsolla is not a Google store.
        /// </summary>
        public IGoogleOrderInfo Google => null;

        /// <summary>
        /// Payment provider specific order details. Always null, Xsolla drives its own checkout.
        /// </summary>
        public IPaymentProvidersOrderInfo PaymentProviders => null;

        /// <summary>
        /// Products carried by the order. Populated by Unity IAP when the order is built.
        /// </summary>
        public List<IPurchasedProductInfo> PurchasedProductInfo { get; set; }

        /// <summary>
        /// Unified receipt understood by Unity IAP, wrapping the Xsolla receipt as its payload.
        /// </summary>
        public string Receipt { get; }

        /// <summary>
        /// Transaction identifier reported by Xsolla.
        /// </summary>
        public string TransactionID { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="XsollaPurchasingOrderInfo"/>.
        /// </summary>
        /// <param name="payload">Xsolla receipt in its serialized form.</param>
        /// <param name="transactionId">Transaction identifier reported by Xsolla.</param>
        public XsollaPurchasingOrderInfo(string payload, string transactionId)
        {
            TransactionID = transactionId ?? string.Empty;
            Receipt = JsonUtility.ToJson(new UnifiedReceipt
            {
                Store = XsollaPurchasingStore.Name,
                TransactionID = TransactionID,
                Payload = payload ?? string.Empty
            });
            PurchasedProductInfo = new List<IPurchasedProductInfo>();
        }
    }
}
#endif
