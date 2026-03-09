using System;

namespace Xsolla.Core
{
    internal static class EventManagerProvider
    {
        public static IEventManagerHandler Handler { get; private set; }
        public static void Register(IEventManagerHandler handler) => Handler = handler;
        public static void Unregister(IEventManagerHandler handler)
        {
            if (Handler == handler) Handler = null;
        }
    }
}