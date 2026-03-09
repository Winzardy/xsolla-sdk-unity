using Xsolla.SDK.Common;

namespace Xsolla.SDK.Store
{
    internal interface IXsollaStoreClient
    {
        void Initialize(XsollaClientConfiguration settings, 
            InitializeResultFunc onSuccess, ErrorFunc onError,
            PurchaseProductResultFunc onSuccessPurchaseProduct, ErrorFunc onErrorPurchase);
        void Deinitialize(DeinitializeResultFunc onSuccess, ErrorFunc onError);
        
        void RestorePurchases(RestorePurchasesResultFunc onSuccess, ErrorFunc onError);
        void FetchProducts(string[] productIds, FetchProductsResultFunc onSuccess, ErrorFunc onError);
        void PurchaseProduct(string sku, string developerPayload, XsollaStoreClientPurchaseArgs args, PurchaseProductResultFunc onSuccess, ErrorFunc onError);
        void ConsumeProduct(string sku, int quantity, string transactionId, ConsumeProductResultFunc onSuccess, ErrorFunc onError);
        void ValidatePurchase(string receipt, ValidatePurchaseResultFunc onSuccess, ErrorFunc onError);
        
        void GetAccessToken(GetAccessTokenResultFunc onSuccess, ErrorFunc onError);
        void UpdateAccessToken(string token, UpdateAccessTokenResultFunc onSuccess, ErrorFunc onError);
        void GetAppleStorefront(GetAppleStorefrontResultFunc onSuccess, ErrorFunc onError);
    }
}