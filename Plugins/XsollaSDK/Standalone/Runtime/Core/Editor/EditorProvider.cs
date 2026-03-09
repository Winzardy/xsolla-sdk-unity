using System;

namespace Xsolla.Core
{
    internal static class EditorProvider
    {
        public static IEditorHandler Handler { get; private set; }
        public static void Register(IEditorHandler handler) => Handler = handler;
        public static void Unregister(IEditorHandler handler)
        {
            if (Handler == handler) Handler = null;
        }
    }
}