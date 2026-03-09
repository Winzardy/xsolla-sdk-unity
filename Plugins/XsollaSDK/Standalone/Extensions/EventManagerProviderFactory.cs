using System;
using UnityEngine;

namespace Xsolla.Core.Extensions
{
    internal static class EventsManagerProviderFactory
    {
        public static IEventManagerHandler Register()
        {
            if (EventManagerProvider.Handler == null)
                EventManagerProvider.Register(new EventsManagerProviderBootstrapHandler());

            return EventManagerProvider.Handler;
        }
        
        public static void Unregister()
        {
            if (EventManagerProvider.Handler != null)
                EventManagerProvider.Unregister(EventManagerProvider.Handler);
        }

        private class EventsManagerProviderBootstrapHandler : IEventManagerHandler
        {
            event Action<string, object> onEvent;
            
            public void Subscribe(Action<string, object> callback) => 
                onEvent += callback;
            
            public void Unsubscribe(Action<string, object> callback) =>
                onEvent -= callback;
            
            public void BroadcastEvent(string eventName) =>
                onEvent?.Invoke(eventName, null);

            public void BroadcastEvent(string eventName, object eventData) =>
                onEvent?.Invoke(eventName, eventData);
        }
    }
}
