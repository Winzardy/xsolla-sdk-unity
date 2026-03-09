using System;

namespace Xsolla.Core
{
    internal interface IEventManagerHandler
    {
        void Subscribe(Action<string, object> callback);
        void Unsubscribe(Action<string, object> callback);
        
        void BroadcastEvent(string eventName);
        void BroadcastEvent(string eventName, object eventData);
    }
}