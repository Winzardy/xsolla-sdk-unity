using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Xsolla.SDK.Utils
{
    internal static class CurrencyFormatter
    {
        public static string ToCurrency(decimal price, [NotNull] string currencyCode, [CanBeNull] CultureInfo referenceLocale)
        {
            NumberFormatInfo numberFormat = null;

            if (referenceLocale != null)
            {
                try
                {
                    var region = new RegionInfo(referenceLocale.Name);
                    if (string.Compare(region.ISOCurrencySymbol, currencyCode, StringComparison.InvariantCultureIgnoreCase) == 0)
                        numberFormat = referenceLocale.NumberFormat;
                }
                catch
                {
                    // do nothing.
                }
            }

            if (numberFormat == null)
            {
                numberFormat = (referenceLocale ?? LocaleInfo.Default.cultureInfo).NumberFormat.Clone() as NumberFormatInfo;
                if (numberFormat != null)
                    numberFormat.CurrencySymbol = CurrencyCodeToSymbol(currencyCode);
            }

            return numberFormat != null
                ? price.ToString("C", AdjustFractionDigits(numberFormat, currencyCode))
                : "N/A";
        }

        private static readonly Dictionary<string, string> currencySymbolCache = new Dictionary<string, string>();

        internal static readonly HashSet<string> ZeroFractionCurrencies = new HashSet<string>()
        {
            "BIF", "CLP", "DJF", "JPY", "KMF", "KRW", "MGA", "PYG",
            "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
        };

        private static string CurrencyCodeToSymbol(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return currencyCode;

            // Check if the currency symbol is already cached
            if (currencySymbolCache.TryGetValue(currencyCode, out string cachedSymbol))
            {
                return cachedSymbol;
            }
            
            try
            {
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    var region = new RegionInfo(culture.Name);
                    if (region.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase))
                    {
                        // Cache the currency symbol for future use
                        currencySymbolCache[currencyCode] = region.CurrencySymbol;
                        
                        return region.CurrencySymbol;
                    }
                }
            }
            catch
            {
                // In case RegionInfo fails, fallback to returning the code
            }

            currencySymbolCache[currencyCode] = currencyCode;
            
            // If not found, return the code itself
            return currencyCode;
        }

        /// <summary>
        /// Adjusts the provided <see cref="NumberFormatInfo"/> to use 0 fraction digits if the given currency code
        /// is known to not have a fractional part.
        /// </summary>
        /// <param name="numberFormat">The <see cref="NumberFormatInfo"/> instance to adjust.</param>
        /// <param name="currencyCode">The ISO 4217 currency code.</param>
        /// <returns>The adjusted <see cref="NumberFormatInfo"/> instance.</returns>
        internal static NumberFormatInfo AdjustFractionDigits(NumberFormatInfo numberFormat, string currencyCode)
        {
            if (!(ZeroFractionCurrencies.Contains(currencyCode) && numberFormat.Clone() is NumberFormatInfo newNumberFormat))
                return numberFormat;

            newNumberFormat.CurrencyDecimalDigits = 0;

            return newNumberFormat;
        }
    }
}