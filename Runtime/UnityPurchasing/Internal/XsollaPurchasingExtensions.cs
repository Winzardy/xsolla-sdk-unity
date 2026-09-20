#if !XSOLLA_SDK_UNITY_PURCHASING_DISABLE
using JetBrains.Annotations;
using System;
using Xsolla.SDK.Common;

namespace Xsolla.SDK.UnityPurchasing
{
    internal static class PurchaseEventArgsExtensions
    {
        /// <summary>
        /// Extracts the Xsolla receipt from the unified receipt Unity IAP reports in <c>Order.Info.Receipt</c>.
        /// </summary>
        /// <returns>The receipt as the SDK serialized it, or null when the string carries no payload.</returns>
        [CanBeNull]
        internal static string ExtractPayloadAsString(string receipt)
        {
            try
            {
                if (string.IsNullOrEmpty(receipt))
                    return null;

                const string PAYLOAD_NEEDLE = "{\"Payload\":\"";
                const string PAYLOAD_TAIL_NEEDLE = "}\",\"Store";

                var payloadKeyIndex = receipt.IndexOf(PAYLOAD_NEEDLE, StringComparison.Ordinal);
                if (payloadKeyIndex < 0)
                    return null;

                var payloadStartIndex = payloadKeyIndex + PAYLOAD_NEEDLE.Length;

                var payloadEndIndex = receipt.LastIndexOf(PAYLOAD_TAIL_NEEDLE, StringComparison.Ordinal);
                if (payloadEndIndex < 0)
                    return null;

                var escapedPayloadStr = receipt.Substring(
                    payloadStartIndex, payloadEndIndex - payloadStartIndex + 1
                );

                var unescapedPayloadStr = UnescapeJsonString(escapedPayloadStr);

                return unescapedPayloadStr;
            }
            catch (Exception e)
            {
                XsollaLogger.Debug("ExtractPayload", e.Message);
                return null;
            }

            static string UnescapeJsonString(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return str;

                var sb = new System.Text.StringBuilder(str.Length);

                for (var i = 0; i < str.Length; i++)
                {
                    var ch = str[i];

                    if (ch == '\\' && i + 1 < str.Length)
                    {
                        var nextCh = str[i + 1];

                        if (nextCh is '"' or '\\')
                        {
                            sb.Append(nextCh);
                            i++;
                            continue;
                        }
                    }

                    sb.Append(ch);
                }

                return sb.ToString();
            }
        }
    }
}
#endif
