using System;
using UnityEngine;

namespace Xsolla.Core
{
    internal interface IDataHandler
    {
        RuntimePlatform GetPlatform();
        bool IsEditor();
        string GetDeviceId();
        string GetDeviceName();
        string GetDeviceModel();
    }
}