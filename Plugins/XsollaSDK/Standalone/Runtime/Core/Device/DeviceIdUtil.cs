using System;
using System.Text;
using UnityEngine;

namespace Xsolla.Core
{
  internal static class DeviceIdUtil
  {
    /// <summary>
    /// Returns a device ID for user authentication in the format required by the Xsolla API.
    /// </summary>
    /// <remarks>[More about the use cases](https://developers.xsolla.com/sdk/unity/authentication/auth-via-device-id/).</remarks>
    public static string GetDeviceId()
    {
      if (DataProvider.GetPlatform() == RuntimePlatform.WindowsPlayer 
          || DataProvider.GetPlatform() == RuntimePlatform.OSXPlayer
          || DataProvider.GetPlatform() == RuntimePlatform.Android
          || DataProvider.GetPlatform() == RuntimePlatform.IPhonePlayer 
          || Application.isEditor)
      {
        var deviceIdStr = DataProvider.GetDeviceId();
        XDebug.Log($"[DeviceIdUtil][GetDeviceId] id={deviceIdStr}");
        return deviceIdStr;
      }

      throw new System.Exception($"Device id is not supported on this platform: {DataProvider.GetPlatform()}");
    }

    public static string GetDeviceName()
    {
      return DataProvider.GetDeviceName();
    }

    public static string GetDeviceModel()
    {
      return DataProvider.GetDeviceModel();
    }

    /// <summary>High quality, fast string hasher.</summary>
    static class XXHash64
    {
      const ulong PRIME64_1 = 11400714785074694791UL;
      const ulong PRIME64_2 = 14029467366897019727UL;
      const ulong PRIME64_3 = 1609587929392839161UL;
      const ulong PRIME64_4 = 9650029242287828579UL;
      const ulong PRIME64_5 = 2870177450012600261UL;

      public static ulong Hash(string input, ulong seed = 0)
      {
        var data = Encoding.UTF8.GetBytes(input);
        var len = data.Length;
        var index = 0;
        ulong hash;

        if (len >= 32) {
          var v1 = seed + PRIME64_1 + PRIME64_2;
          var v2 = seed + PRIME64_2;
          var v3 = seed;
          var v4 = seed - PRIME64_1;

          while (index <= len - 32) {
            v1 = Round(v1, BitConverter.ToUInt64(data, index));
            index += 8;
            v2 = Round(v2, BitConverter.ToUInt64(data, index));
            index += 8;
            v3 = Round(v3, BitConverter.ToUInt64(data, index));
            index += 8;
            v4 = Round(v4, BitConverter.ToUInt64(data, index));
            index += 8;
          }

          hash = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
          hash = MergeRound(hash, v1);
          hash = MergeRound(hash, v2);
          hash = MergeRound(hash, v3);
          hash = MergeRound(hash, v4);
        } else {
          hash = seed + PRIME64_5;
        }

        hash += (ulong)len;

        while (index + 8 <= len) {
          var k1 = BitConverter.ToUInt64(data, index);
          hash ^= Round(0, k1);
          hash = RotateLeft(hash, 27) * PRIME64_1 + PRIME64_4;
          index += 8;
        }

        while (index < len) {
          hash ^= data[index] * PRIME64_5;
          hash = RotateLeft(hash, 11) * PRIME64_1;
          index++;
        }

        return Avalanche(hash);

        static ulong Round(ulong acc, ulong input)
        {
          acc += input * PRIME64_2;
          acc = RotateLeft(acc, 31);
          acc *= PRIME64_1;

          return acc;
        }

        static ulong MergeRound(ulong acc, ulong val)
        {
          val = Round(0, val);
          acc ^= val;
          acc = acc * PRIME64_1 + PRIME64_4;

          return acc;
        }

        static ulong Avalanche(ulong h)
        {
          h ^= h >> 33;
          h *= PRIME64_2;
          h ^= h >> 29;
          h *= PRIME64_3;
          h ^= h >> 32;

          return h;
        }

        static ulong RotateLeft(ulong x, int r) => (x << r) | (x >> (64 - r));
      }
    }
  }
}
