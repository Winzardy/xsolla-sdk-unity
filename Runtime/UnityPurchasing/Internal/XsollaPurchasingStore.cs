#if !XSOLLA_SDK_UNITY_PURCHASING_DISABLE
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Xsolla.SDK.Utils;
using Xsolla.SDK.Store;
using Xsolla.SDK.Common;

namespace Xsolla.SDK.UnityPurchasing
{
    using UnityStore = UnityEngine.Purchasing.Extension.Store;

    /// <summary>
    /// XsollaPurchasingStore
    /// </summary>
    internal class XsollaPurchasingStore : UnityStore, IXsollaPurchasingStoreExtension
    {
        public const string Name = "XsollaStore";
        
        private const string Tag = "XsollaPurchasingStore";
        
        private readonly IXsollaStoreClient _storeClient = XsollaStoreClientFactory.Create();

        private readonly ISimpleFuture<XsollaClientConfiguration, string> _settingsFuture;
        [CanBeNull] private ISimpleFuture<bool, string> _connectedFuture;
        [CanBeNull] private ISimpleFuture<bool, string> _productsFuture;
        
        private readonly Dictionary<string, XsollaStoreClientProduct> _productById = new Dictionary<string, XsollaStoreClientProduct>();

        /// <summary>
        /// Purchased products reported by the last restore, by SKU. Backs entitlement checks.
        /// </summary>
        private readonly Dictionary<string, XsollaStoreClientPurchasedProduct> _purchasedById =
            new Dictionary<string, XsollaStoreClientPurchasedProduct>();

        [CanBeNull] private Action<IStorePurchaseCallback, PendingOrder> onPurchaseSucceeded;
        [CanBeNull] private Action<IStorePurchaseCallback, FailedOrder> onPurchaseFailed;

        [CanBeNull] private XsollaPurchaseArgsProvider purchaseArgsProvider;

        [CanBeNull] private XsollaPurchasingStoreValidator _validator;

        /// <summary>
        /// Connection state reported to Unity IAP through the store wrapper.
        /// </summary>
        public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

        public XsollaPurchasingStore(XsollaClientConfiguration configuration) {
            RunOnStartThread.Create();
            
            _settingsFuture = SimpleFuture.Create<XsollaClientConfiguration, string>(out var promise);
            XsollaLogger.SetLogLevel(configuration.logLevel);
            if (configuration.delayedTask != null)
            {
                AwaitForConfiguration(configuration, promise);
            }
            else 
                promise.Complete(configuration);
        }
        
        private async void AwaitForConfiguration(XsollaClientConfiguration configuration, ISimplePromise<XsollaClientConfiguration, string> promise)
        {
            var mapper = await configuration.delayedTask;
            RunOnStartThread.Run(() => promise.Complete(mapper(configuration)));
        }

        #region Store

        public override void Connect()
        {
            XsollaLogger.Debug(Tag, "Connect");

            ConnectionState = ConnectionState.Connecting;

            var client = _storeClient;
            
            _connectedFuture = SimpleFuture.Create<bool, string>(out var promise);
            _settingsFuture.OnComplete(
                onSuccess: configuration => {
                    XsollaLogger.SetLogLevel(configuration.logLevel);
                    
                    client.Initialize(
                        configuration, 
                        onSuccess: () => {
                            XsollaLogger.Debug(Tag, "Connect finished");

                            ConnectionState = ConnectionState.Connected;
                            promise.Complete(true);
                            ConnectCallback?.OnStoreConnectionSucceeded();
                        }, 
                        onError: error => OnConnectFailed(error, promise),
                        onSuccessPurchaseProduct: OnPurchaseSucceeded, 
                        onErrorPurchase: error => OnPurchaseFailed(error, null)
                    );
                },
                onError: error => OnConnectFailed(error, promise)
            );
        }

        private void OnConnectFailed(string error, ISimplePromise<bool, string> promise)
        {
            XsollaLogger.Error(Tag, $"Connect failed: {error}");

            ConnectionState = ConnectionState.Disconnected;
            promise.CompleteWithError(error);

            // Retries are driven by the application, Unity IAP would otherwise loop on a policy of its own
            ConnectCallback?.OnStoreConnectionFailed(new StoreConnectionFailureDescription(error, isRetryable: false));
        }

        public override void FetchProducts(IReadOnlyCollection<ProductDefinition> products)
        {
            XsollaLogger.Debug(Tag, "FetchProducts");
            
            if (_connectedFuture == null) {
                XsollaLogger.Error(Tag, "FetchProducts: not connected");
                OnProductsFetchFailed(ProductFetchFailureReason.ProviderUnavailable, "Store is not connected");
                return;
            }
            
            _productsFuture = SimpleFuture.Create<bool, string>(out var promise);

            _connectedFuture.OnComplete(
                onSuccess: _ => FetchProducts_(),
                onError: error => {
                    XsollaLogger.Error(Tag, $"FetchProducts failed: {error}");

                    promise.Complete(false);
                    OnProductsFetchFailed(ProductFetchFailureReason.ProviderUnavailable, error);
                }
            );

            void FetchProducts_() {
                var skus = new string[products.Count];
                var index = 0;
                foreach (var product in products)
                    skus[index++] = product.storeSpecificId;

                _storeClient.FetchProducts(skus,
                    onSuccess: items => {
                        XsollaLogger.Debug(Tag, $"FetchProducts finished successfully. " +
                                        $"Retrieved: {items.Length} ( {XsollaClientHelpers.ToJson(items)} )");

                        //Store url for future use
                        foreach (var item in items)
                            _productById[item.sku] = item;

                        var descriptions = new List<ProductDescription>(items.Length);
                        foreach (var item in items)
                        {
                            descriptions.Add(new ProductDescription(
                                id: item.sku,
                                metadata: new ProductMetadata(
                                    priceString: item.localizedPriceString,
                                    title: item.localizedTitle,
                                    description: item.localizedDescription,
                                    currencyCode: item.currencyCode,
                                    localizedPrice: (decimal) item.localizedPrice / 1_000_000
                                )
                            ));
                        }

                        promise.Complete(true);
                        ProductsCallback?.OnProductsFetched(descriptions);
                    },
                    onError: error => {
                        XsollaLogger.Error(Tag, $"FetchProducts failed: {error}");

                        promise.Complete(false);
                        OnProductsFetchFailed(ProductFetchFailureReason.ProductsUnavailable, error);
                    }
                );
            }
        }

        private void OnProductsFetchFailed(ProductFetchFailureReason reason, string message)
            // Retries are driven by the application, the default Unity IAP policy never gives up
            => ProductsCallback?.OnProductsFetchFailed(new ProductFetchFailureDescription(reason, message, isRetryable: false));

        public override void FetchPurchases()
        {
            XsollaLogger.Debug(Tag, "FetchPurchases");

            // Orders are built from the products Unity IAP has cached, so the catalog goes first
            if (_productsFuture == null)
            {
                XsollaLogger.Error(Tag, "FetchPurchases: products are not fetched");
                OnPurchasesFetchFailed(PurchasesFetchFailureReason.StoreNotConnected, "Products are not fetched");
                return;
            }

            _productsFuture.OnComplete(
                onSuccess: _ => FetchPurchases_(),
                onError: error => {
                    XsollaLogger.Error(Tag, $"FetchPurchases failed: {error}");
                    OnPurchasesFetchFailed(PurchasesFetchFailureReason.Unknown, error);
                }
            );

            void FetchPurchases_() =>
                _storeClient.RestorePurchases(
                    onSuccess: items => {
                        XsollaLogger.Debug(Tag, $"FetchPurchases finished successfully. Restored: {items.Length}");

                        var orders = new List<Order>(items.Length);
                        foreach (var item in items)
                        {
                            _purchasedById[item.sku] = item;

                            if (TryCreateOrder(item, out var order))
                                orders.Add(order);
                            else
                                XsollaLogger.Warning(Tag, $"FetchPurchases: skipped unknown product by sku [ {item.sku} ]");
                        }

                        PurchaseFetchCallback?.OnAllPurchasesRetrieved(orders);
                    },
                    onError: error => {
                        XsollaLogger.Error(Tag, $"FetchPurchases failed: {error}");
                        OnPurchasesFetchFailed(PurchasesFetchFailureReason.Unknown, error);
                    }
                );
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureReason reason, string message)
            => PurchaseFetchCallback?.OnPurchasesRetrievalFailed(new PurchasesFetchFailureDescription(reason, message));

        public override void Purchase(ICart cart)
        {
            var item = GetFirstItem(cart);

            if (item == null)
            {
                XsollaLogger.Error(Tag, "Purchase: cart is empty");
                PurchaseCallback?.OnPurchaseFailed(
                    new FailedOrder(cart, PurchaseFailureReason.ProductUnavailable, "Cart is empty"));
                return;
            }

            var sku = GetStoreSpecificId(item);

            if (purchaseArgsProvider == null)
            {
                Purchase(sku, null, XsollaStoreClientPurchaseArgs.Empty);
                return;
            }

            purchaseArgsProvider(
                sku,
                onReady: args =>
                {
                    if (args == null)
                    {
                        OnPurchaseCanceled(sku);
                        return;
                    }

                    Purchase(sku, null, args);
                },
                onError: error => OnPurchaseFailed(error, sku)
            );
        }

        /// <summary>
        /// Reports a purchase the arguments provider refused to prepare, the user backed out of it.
        /// </summary>
        private void OnPurchaseCanceled(string sku)
        {
            XsollaLogger.Warning(Tag, $"Purchase canceled, sku: {sku}");

            if (!TryCreateCart(sku, out var cart))
            {
                XsollaLogger.Error(Tag, $"Purchase canceled: unknown product by sku [ {sku} ]");
                return;
            }

            var failedOrder = new FailedOrder(cart, PurchaseFailureReason.UserCancelled, "Purchase canceled");

            if (onPurchaseFailed != null)
                onPurchaseFailed(PurchaseCallback, failedOrder);
            else
                PurchaseCallback?.OnPurchaseFailed(failedOrder);
        }
        
        private void Purchase(string sku, string developerPayload, XsollaStoreClientPurchaseArgs args)
        {
            XsollaLogger.Debug(Tag, "Purchase");

            _storeClient.PurchaseProduct(sku, developerPayload, args,
                onSuccess: OnPurchaseSucceeded,
                onError: error => OnPurchaseFailed(error, sku)
            );
        }
        
        private void OnPurchaseSucceeded(XsollaStoreClientPurchasedProduct product)
        {
            XsollaLogger.Debug(Tag, $"Purchase finished: {product}");

            _productsFuture?.OnComplete(
                onSuccess: loaded =>
                {
                    _purchasedById[product.sku] = product;

                    if (!TryCreateOrder(product, out var order))
                    {
                        XsollaLogger.Error(Tag, $"OnPurchaseSucceeded failed: unknown product by sku [ {product.sku} ]");
                        return;
                    }

                    if (onPurchaseSucceeded != null)
                        onPurchaseSucceeded(PurchaseCallback, order);
                    else
                        PurchaseCallback?.OnPurchaseSucceeded(order);
                },
                onError: error =>
                {
                    XsollaLogger.Error(Tag, $"OnPurchaseSucceeded failed: {error}");
                }
            );
        }
        
        private void OnPurchaseFailed(string error, [CanBeNull] string sku)
        {
            var parsed = XsollaStoreClientHelpers.ParsePurchaseError(error);
            var reason = MapToIapReason(parsed.code);

            if (reason == PurchaseFailureReason.UserCancelled) {
                XsollaLogger.Warning(Tag, $"Purchase failed: {parsed.message}, reason: {reason}, sku: {sku}");
            } else {
                XsollaLogger.Error(Tag, $"Purchase failed: {parsed.message}, reason: {reason}, sku: {sku}");
            }

            if (sku == null) return;

            if (!TryCreateCart(sku, out var cart))
            {
                XsollaLogger.Error(Tag, $"Purchase failed: unknown product by sku [ {sku} ]");
                return;
            }

            var failedOrder = new FailedOrder(cart, reason, parsed.message);
                    
            if (onPurchaseFailed != null)
                onPurchaseFailed(PurchaseCallback, failedOrder);
            else 
                PurchaseCallback?.OnPurchaseFailed(failedOrder);
        }

        private PurchaseFailureReason MapToIapReason(XsollaStoreClientPurchaseErrorCode code)
        {
            switch (code)
            {
                case XsollaStoreClientPurchaseErrorCode.Cancelled:
                    return PurchaseFailureReason.UserCancelled;
                default:
                    return PurchaseFailureReason.PaymentDeclined;
            }
        }

        public override void FinishTransaction(PendingOrder pendingOrder)
        {
            XsollaLogger.Debug(Tag, "FinishTransaction");

            var transactionId = pendingOrder.Info.TransactionID;
            var item = GetFirstItem(pendingOrder.CartOrdered);

            if (item == null)
            {
                XsollaLogger.Error(Tag, "FinishTransaction: cart is empty");
                ConfirmCallback?.OnConfirmOrderFailed(
                    new FailedOrder(pendingOrder, PurchaseFailureReason.PurchaseMissing, "Cart is empty"));
                return;
            }

            if (item.Product.type != ProductType.Consumable)
            {
                XsollaLogger.Debug(Tag, "FinishTransaction: Product is not consumable. Ignoring.");
                ConfirmCallback?.OnConfirmOrderSucceeded(transactionId);
                return;
            }

            _storeClient.ConsumeProduct(GetStoreSpecificId(item), 1, transactionId,
                onSuccess: () => {
                    XsollaLogger.Debug(Tag, $"FinishTransaction finished");
                    ConfirmCallback?.OnConfirmOrderSucceeded(transactionId);
                },
                onError: error => {
                    XsollaLogger.Error(Tag, $"FinishTransaction failed: {error}");
                    ConfirmCallback?.OnConfirmOrderFailed(
                        new FailedOrder(pendingOrder, PurchaseFailureReason.Unknown, error));
                }
            );
        }

        public override void CheckEntitlement(ProductDefinition product)
        {
            XsollaLogger.Debug(Tag, "CheckEntitlement");

            _storeClient.RestorePurchases(
                onSuccess: items => {
                    foreach (var item in items)
                        _purchasedById[item.sku] = item;

                    var entitled = _purchasedById.ContainsKey(product.storeSpecificId);
                    EntitlementCallback?.OnCheckEntitlement(product,
                        entitled ? EntitlementStatus.FullyEntitled : EntitlementStatus.NotEntitled);
                },
                onError: error => {
                    XsollaLogger.Error(Tag, $"CheckEntitlement failed: {error}");
                    EntitlementCallback?.OnCheckEntitlement(product, EntitlementStatus.Unknown, error);
                }
            );
        }

        #endregion

        #region Orders

        private bool TryCreateOrder(XsollaStoreClientPurchasedProduct purchased, out PendingOrder order)
        {
            order = null;

            if (!TryCreateCart(purchased.sku, out var cart))
                return false;

            var info = new XsollaPurchasingOrderInfo(purchased.ToReceipt().ToJson(), purchased.transactionId);
            order = new PendingOrder(cart, info);
            return true;
        }

        private bool TryCreateCart(string sku, out ICart cart)
        {
            cart = null;

            var product = ReadOnlyProductCache.FindByStoreSpecificId(sku);
            if (product == null)
                return false;

            cart = new Cart(product);
            return true;
        }

        [CanBeNull]
        private static CartItem GetFirstItem(ICart cart)
        {
            var items = cart?.Items();
            return items == null || items.Count == 0 ? null : items[0];
        }

        private static string GetStoreSpecificId(CartItem item)
            => item.Product.catalogListings.TryGetValue(item.CatalogListingId, out var listing) && listing.definition != null
                ? listing.definition.storeSpecificId
                : item.Product.definition?.storeSpecificId;

        #endregion

        #region Extension

        public void RestoreTransactions(Action<bool> onSuccess, Action<string> onError)
        {
            XsollaLogger.Debug(Tag, "RestoreTransactions");
            
            _storeClient.RestorePurchases(
                onSuccess: items =>
                {
                    foreach (var item in items)
                    {
                        //if (item.VirtualItemType != VirtualItemType.Consumable)
                        //    continue;
                        
                        OnPurchaseSucceeded(item);
                    }
                    
                    onSuccess?.Invoke(items.Length > 0);
                },
                onError: error => onError?.Invoke(error)
            );
        }

        public bool TryGetProductIconUrl(Product product, out string url)
        {
            var res = _productById.TryGetValue(product.definition.storeSpecificId, out var productData);
            if (res)
            {
                url = productData.iconUrl;
                return true;
            }
            
            url = null;
            return false;
        }

        public bool TryGetProduct(Product product, out XsollaStoreClientProduct productData)
        {
            return _productById.TryGetValue(product.definition.storeSpecificId, out productData);
        }

        public void SetPurchaseArgsProvider([CanBeNull] XsollaPurchaseArgsProvider provider)
        {
            purchaseArgsProvider = provider;
        }

        public void SetCustomPurchaseFlowCallbacks(
            [CanBeNull] Action<IStorePurchaseCallback, PendingOrder> onPurchaseSucceeded,
            [CanBeNull] Action<IStorePurchaseCallback, FailedOrder> onPurchaseFailed
        ) {
            this.onPurchaseSucceeded = onPurchaseSucceeded;
            this.onPurchaseFailed = onPurchaseFailed;
        }

        public XsollaPurchasingStoreValidator GetValidator()
        {
            if (_validator == null)
                _validator = new XsollaPurchasingStoreValidator(_storeClient);
            return _validator;
        }

        public void GetAccessToken(Action<string> onSuccess, Action<string> onError)
        {
            XsollaLogger.Debug(Tag, "GetAccessToken");
            
            _storeClient.GetAccessToken(
                onSuccess: token => onSuccess?.Invoke(token),
                onError: error => onError?.Invoke(error)
            );
        }

        public void UpdateAccessToken(string token, Action onSuccess, Action<string> onError)
        {
            XsollaLogger.Debug(Tag, "UpdateAccessToken");
            
            _storeClient.UpdateAccessToken(
                token,
                onSuccess: () => onSuccess?.Invoke(),
                onError: error => onError?.Invoke(error)
            );
        }

        public void GetAppleStorefront(Action<string> onSuccess, Action<string> onError)
        {
            XsollaLogger.Debug(Tag, "GetAppleStorefront");
            
            _storeClient.GetAppleStorefront(
                onSuccess: storefront => onSuccess?.Invoke(storefront),
                onError: error => onError?.Invoke(error)
            );
        }

        public void InitiatePurchase(Product product, XsollaStoreClientPurchaseArgs args)
        {
            Purchase(product.definition.storeSpecificId, null, args);
        }

        public void InitiatePurchase(string productId, XsollaStoreClientPurchaseArgs args)
        {
            Purchase(productId, null, args);
        }

        #endregion
    }
}
#endif
