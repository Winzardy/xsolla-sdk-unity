using System;
using Xsolla.SDK.Store;

namespace Xsolla.SDK.Extensions
{
    internal class XsollaStoreClientEventManager
    {
        protected event Action<XsollaStoreClientEventTypes, object> onEvent;

        protected XsollaStoreClientEventManager() {}

        internal class Builder
        {
            protected XsollaStoreClientEventManager _manager = new XsollaStoreClientEventManager();

            public Builder SetOnEventCallback(Action<XsollaStoreClientEventTypes, object> callback)
            {
                _manager.onEvent += callback;
                return this;
            }
            
            public static Builder Create() => new Builder();
            public XsollaStoreClientEventManager Build() => _manager;

            public XsollaStoreClientEventManager Start()
            {
                var namager = Build();
                namager.Start();
                return namager;
            } 
        }

        public void AddOnEvent(Action<XsollaStoreClientEventTypes, object> callback) =>
            onEvent += callback;
        public void RemoveOnEvent(Action<XsollaStoreClientEventTypes, object> callback) =>
            onEvent -= callback;

        public void Stop() => Stop_();

        private void Start()
        {
#if UNITY_STANDALONE || UNITY_WEBGL
            Xsolla.Core.Extensions.EventsManagerProviderFactory.Register().Subscribe(onEvents_);
#elif UNITY_ANDROID
            XsollaStoreClientImplAndroid.onNativePaymentEvent += onEvents_;
#elif UNITY_IOS
        XsollaStoreClientImplIOS.onNativePaymentEvent += onEvents_;
#endif
        }
        
        private void Stop_()
        {
#if UNITY_STANDALONE || UNITY_WEBGL
            Xsolla.Core.Extensions.EventsManagerProviderFactory.Register().Unsubscribe(onEvents_);
#elif UNITY_ANDROID
            XsollaStoreClientImplAndroid.onNativePaymentEvent -= onEvents_;
#elif UNITY_IOS
        XsollaStoreClientImplIOS.onNativePaymentEvent -= onEvents_;
#endif
        }
        
        private void onEvents_(string eventName, object eventData)
        {
            switch (eventName)
            {
                case "PaystationOpen":
                    onEvent?.Invoke(XsollaStoreClientEventTypes.PaystationOpen, eventData);
                    break;
                case "PaystationLoaded":
                    onEvent?.Invoke(XsollaStoreClientEventTypes.PaystationLoaded, eventData);
                    break;
                case "PaystationCancelled":
                    onEvent?.Invoke(XsollaStoreClientEventTypes.PaystationCancelled, eventData);
                    break;
                case "PaystationCompleted":
                    onEvent?.Invoke(XsollaStoreClientEventTypes.PaystationCompleted, eventData);
                    break;
            }
        }
    }
}