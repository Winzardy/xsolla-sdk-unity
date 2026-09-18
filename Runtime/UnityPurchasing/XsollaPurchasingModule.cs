#if !XSOLLA_SDK_UNITY_PURCHASING_DISABLE
using JetBrains.Annotations;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Xsolla.SDK.Common;

namespace Xsolla.SDK.UnityPurchasing
{
    using UnityStore = UnityEngine.Purchasing.Extension.Store;

    /// <summary>
    /// Provides a Unity Purchasing store for Xsolla integration.
    /// </summary>
    /// <remarks>
    /// Register the module with <c>UnityIAPServices.AddNewCustomStore(module)</c> before asking for
    /// a store controller by <see cref="StoreName"/>.
    /// </remarks>
    public class XsollaPurchasingModule : IStoreWrapper
    {
        /// <summary>
        /// Gets the store name for Xsolla Purchasing.
        /// </summary>
        public static string StoreName => XsollaPurchasingStore.Name;

        /// <summary>
        /// Builder for <see cref="XsollaPurchasingModule"/>.
        /// </summary>
        public class Builder
        {
            private XsollaClientConfiguration _configuration = XsollaClientConfiguration.Builder.Empty();

            /// <summary>
            /// Creates a new builder instance.
            /// </summary>
            public static Builder Create() => new Builder();

            /// <summary>
            /// Sets the client configuration.
            /// </summary>
            /// <param name="configuration">Client configuration.</param>
            public Builder SetConfiguration(XsollaClientConfiguration configuration) { _configuration = configuration; return this; }

            /// <summary>
            /// Builds the <see cref="XsollaPurchasingModule"/> instance.
            /// </summary>
            public XsollaPurchasingModule Build() => new XsollaPurchasingModule(_configuration);
        }

        /// <summary>
        /// The client configuration for the purchasing module.
        /// </summary>
        [CanBeNull] private readonly XsollaClientConfiguration _configuration;

        private readonly XsollaPurchasingStore _store;

        /// <summary>
        /// Initializes a new instance of <see cref="XsollaPurchasingModule"/>.
        /// </summary>
        /// <param name="configuration">Client configuration.</param>
        private XsollaPurchasingModule(XsollaClientConfiguration configuration)
        {
            _configuration = configuration;

            XsollaLogger.SetLogLevel(_configuration.logLevel);

            _store = new XsollaPurchasingStore(_configuration);
        }

        /// <summary>
        /// Xsolla specific store API, replaces the extension provider removed in Unity IAP 5.
        /// </summary>
        public IXsollaPurchasingStoreExtension Extension => _store;

        #region IStoreWrapper

        /// <summary>
        /// Gets the instance of the store.
        /// </summary>
        public UnityStore instance => _store;

        /// <summary>
        /// Gets the name of the store.
        /// </summary>
        public string name => StoreName;

        /// <summary>
        /// Gets the connection state of the store.
        /// </summary>
        public ConnectionState GetStoreConnectionState() => _store.ConnectionState;

        #endregion
    }
}
#endif
