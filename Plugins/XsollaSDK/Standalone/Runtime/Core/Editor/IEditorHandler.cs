using System;

namespace Xsolla.Core
{
    internal interface IEditorHandler
    {
        string GetActiveBuildTargetAsString();
        
        void SubscribeOnDeeplinkEvent(Action<string> callback);
        void UnsubscribeOnDeeplinkEvent(Action<string> callback);
        void OnDeeplinkEvent(string url);
        
        string DeeplinkUrl { get; set; }
    }
}