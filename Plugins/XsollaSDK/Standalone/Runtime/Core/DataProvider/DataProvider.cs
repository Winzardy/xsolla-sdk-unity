using System;
using UnityEngine;

namespace Xsolla.Core
{
    internal static class DataProvider
    {
        private static IDataHandler Handler { get; set; }
        public static void Register(IDataHandler handler) => Handler = handler;
        public static void Unregister(IDataHandler handler)
        {
            if (Handler == handler) Handler = null;
        }
        
        public static RuntimePlatform GetPlatform()
        {
            if (Handler != null)
                return Handler.GetPlatform();
            
            return Application.platform;
        }

        public static bool IsEditor()
        {
            if (Handler != null)
                return Handler.IsEditor();
            
            return Application.isEditor;
        }

        public static string GetDeviceId()
        {
            if (Handler != null)
                return Handler.GetDeviceId();
            
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static string GetDeviceName()
        {
            if (Handler != null)
                return Handler.GetDeviceName();

            return SystemInfo.deviceName;
        }

        public static string GetDeviceModel()
        {
            if (Handler != null)
                return Handler.GetDeviceModel();
            
            return SystemInfo.deviceModel;
        }
    }
}