namespace Xsolla.SDK.Store
{
    /// <summary>
    /// Delegate for error callbacks.
    /// </summary>
    /// <param name="error">Error message string.</param>
    public delegate void ErrorFunc(string error);

    /// <summary>
    /// Delegate for successful initialization callbacks.
    /// </summary>
    public delegate void InitializeResultFunc();

    /// <summary>
    /// Delegate for successful deinitialization callbacks.
    /// </summary>
    public delegate void DeinitializeResultFunc();

    /// <summary>
    /// Delegate for restore purchases result callbacks.
    /// </summary>
    /// <param name="items">Array of restored purchased products.</param>
    public delegate void RestorePurchasesResultFunc(XsollaStoreClientPurchasedProduct[] items);

    /// <summary>
    /// Delegate for fetch products result callbacks.
    /// </summary>
    /// <param name="items">Array of fetched store client products.</param>
    public delegate void FetchProductsResultFunc(XsollaStoreClientProduct[] items);

    /// <summary>
    /// Delegate for purchase product result callbacks.
    /// </summary>
    /// <param name="product">Purchased product.</param>
    public delegate void PurchaseProductResultFunc(XsollaStoreClientPurchasedProduct product);

    /// <summary>
    /// Delegate for consume product result callbacks.
    /// </summary>
    public delegate void ConsumeProductResultFunc();

    /// <summary>
    /// Delegate for validate purchase result callbacks.
    /// </summary>
    /// <param name="result">Validation result (true if valid).</param>
    public delegate void ValidatePurchaseResultFunc(bool result);

    /// <summary>
    /// Delegate for get access token result callbacks.
    /// </summary>
    /// <param name="token">Access token string.</param>
    public delegate void GetAccessTokenResultFunc(string token);

    /// <summary>
    /// Delegate for get Apple Storefront result callbacks.
    /// </summary>
    /// <param name="storefront">Apple Storefront identifier string.</param>
    public delegate void GetAppleStorefrontResultFunc(string storefront);

    /// <summary>
    /// Delegate for get Apple Distribution result callbacks.
    /// </summary>
    /// <param name="isRunningInAlternativeDistribution">Whether app is running in alternative distribution.</param>
    public delegate void GetAppleDistributionResultFunc(bool isRunningInAlternativeDistribution);
    
    public delegate void UpdateAccessTokenResultFunc();
}