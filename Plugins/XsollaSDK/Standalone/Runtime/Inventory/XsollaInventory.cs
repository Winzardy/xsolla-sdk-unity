using System;
using System.Collections.Generic;
using Xsolla.Core;

namespace Xsolla.Inventory
{
	internal static class XsollaInventory
	{
		private const string BaseUrl = "https://store.xsolla.com/api/v2/project";

		/// <summary>
		/// Returns the current user’s balance of virtual currency. For each virtual currency, complete data is returned.
		/// </summary>
		/// <remarks>[More about the use cases](https://developers.xsolla.com/sdk/unity/player-inventory/display-inventory/).</remarks>
		/// <param name="onSuccess">Called after server response.</param>
		/// <param name="onError">Called after virtual currency balance was successfully received.</param>
		/// <param name="platform">Publishing platform the user plays on.<br/>
		///     Can be `xsolla` (default), `playstation_network`, `xbox_live`, `pc_standalone`, `nintendo_shop`, `google_play`, `app_store_ios`, `android_standalone`, `ios_standalone`, `android_other`, `ios_other`, or `pc_other`.</param>
		public static void GetVirtualCurrencyBalance(XsollaSettings settings, Action<VirtualCurrencyBalances> onSuccess, Action<Error> onError, string platform = null)
		{
			var url = new UrlBuilder($"{BaseUrl}/{settings.StoreProjectId}/user/virtual_currency_balance")
				.AddPlatform(platform)
				.Build();

			WebRequestHelper.Instance.GetRequest(
				SdkType.Store,
				url,
				WebRequestHeader.AuthHeader(settings),
				onSuccess,
				error => TokenAutoRefresher.Check(settings, error, onError, () => GetVirtualCurrencyBalance(settings, onSuccess, onError, platform)),
				ErrorGroup.ItemsListErrors);
		}

		/// <summary>
		/// Returns a list of virtual items from the user’s inventory according to pagination settings. For each virtual item, complete data is returned.
		/// <b>Attention:</b> The number of items returned in a single response is limited. <b>The default and maximum value is 50 items per response</b>. To get more data page by page, use <code>limit</code> and <code>offset</code> fields.
		/// </summary>
		/// <remarks>[More about the use cases](https://developers.xsolla.com/sdk/unity/player-inventory/display-inventory/).</remarks>
		/// <param name="onSuccess">Called after purchased items were successfully received.</param>
		/// <param name="onError">Called after the request resulted with an error.</param>
		/// <param name="limit">Limit for the number of elements on the page. The maximum number of elements on a page is 50.</param>
		/// <param name="offset">Number of the element from which the list is generated (the count starts from 0).</param>
		/// <param name="locale">Defines localization of item's text fields. [Two-letter lowercase language code](https://developers.xsolla.com/doc/pay-station/features/localization/). Leave empty to use the default value.</param>
		/// <param name="platform">Publishing platform the user plays on.<br/>
		///     Can be `xsolla` (default), `playstation_network`, `xbox_live`, `pc_standalone`, `nintendo_shop`, `google_play`, `app_store_ios`, `android_standalone`, `ios_standalone`, `android_other`, `ios_other`, or `pc_other`.</param>
		public static void GetInventoryItems(XsollaSettings settings, Action<InventoryItems> onSuccess, Action<Error> onError, int limit = 50, int offset = 0, string locale = null, string platform = null)
		{
			var url = new UrlBuilder($"{BaseUrl}/{settings.StoreProjectId}/user/inventory/items")
				.AddLimit(limit)
				.AddOffset(offset)
				.AddLocale(locale)
				.AddPlatform(platform)
				.Build();

			WebRequestHelper.Instance.GetRequest(
				SdkType.Store,
				url,
				WebRequestHeader.AuthHeader(settings),
				onSuccess,
				error => TokenAutoRefresher.Check(settings, error, onError, () => GetInventoryItems(settings, onSuccess, onError, limit, offset, locale, platform)),
				ErrorGroup.ItemsListErrors);
		}

		/// <summary>
		/// Consumes an inventory item. Use for only for consumable virtual items.
		/// </summary>
		/// <remarks>[More about the use cases](https://developers.xsolla.com/sdk/unity/player-inventory/consume-item/).</remarks>
		/// <param name="item">Contains consume parameters.</param>
		/// <param name="onSuccess">Called after successful inventory item consumption.</param>
		/// <param name="onError">Called after the request resulted with an error.</param>
		/// <param name="platform">Publishing platform the user plays on.<br/>
		///     Can be `xsolla` (default), `playstation_network`, `xbox_live`, `pc_standalone`, `nintendo_shop`, `google_play`, `app_store_ios`, `android_standalone`, `ios_standalone`, `android_other`, `ios_other`, or `pc_other`.</param>
		public static void ConsumeInventoryItem(XsollaSettings settings, ConsumeItem item, Action onSuccess, Action<Error> onError, string platform = null)
		{
			var url = new UrlBuilder($"{BaseUrl}/{settings.StoreProjectId}/user/inventory/item/consume")
				.AddPlatform(platform)
				.Build();

			var headers = new List<WebRequestHeader> {
				WebRequestHeader.AuthHeader(settings),
				WebRequestHeader.JsonContentTypeHeader()
			};

			WebRequestHelper.Instance.PostRequest(
				SdkType.Store,
				url,
				item,
				headers,
				onSuccess,
				error => TokenAutoRefresher.Check(settings, error, onError, () => ConsumeInventoryItem(settings, item, onSuccess, onError, platform)),
				ErrorGroup.ConsumeItemErrors);
		}

		/// <summary>
		/// Returns a list of time-limited items from the user’s inventory. For each item, complete data is returned.
		/// </summary>
		/// <remarks>[More about the use cases](https://developers.xsolla.com/sdk/unity/player-inventory/display-inventory/).</remarks>
		/// <param name="onSuccess">Called after list of user time limited items was successfully received.</param>
		/// <param name="onError">Called after the request resulted with an error.</param>
		/// <param name="platform">Publishing platform the user plays on.<br/>
		///     Can be `xsolla` (default), `playstation_network`, `xbox_live`, `pc_standalone`, `nintendo_shop`, `google_play`, `app_store_ios`, `android_standalone`, `ios_standalone`, `android_other`, `ios_other`, or `pc_other`.</param>
		public static void GetTimeLimitedItems(XsollaSettings settings, Action<TimeLimitedItems> onSuccess, Action<Error> onError, string platform = null)
		{
			var url = new UrlBuilder($"{BaseUrl}/{settings.StoreProjectId}/user/time_limited_items")
				.AddPlatform(platform)
				.Build();

			WebRequestHelper.Instance.GetRequest(
				SdkType.Store,
				url,
				WebRequestHeader.AuthHeader(settings),
				onSuccess,
				error => TokenAutoRefresher.Check(settings, error, onError, () => GetTimeLimitedItems(settings, onSuccess, onError, platform)),
				ErrorGroup.ItemsListErrors);
		}
	}
}
