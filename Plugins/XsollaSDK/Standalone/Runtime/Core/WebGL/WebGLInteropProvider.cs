using System;

namespace Xsolla.Core
{
    internal static class WebGLInteropHProvider
    {
        public static IWebGLInteropHandler Handler { get; private set; }
        public static void Register(IWebGLInteropHandler handler) => Handler = handler;
        public static void Unregister(IWebGLInteropHandler handler)
        {
            if (Handler == handler) Handler = null;
        }
    }
}