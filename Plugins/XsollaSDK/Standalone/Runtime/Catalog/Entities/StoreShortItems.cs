using System;
using Xsolla.Core;

namespace Xsolla.Catalog
{
	[Serializable]
	internal class StoreShortItems
	{
		public StoreShortItem[] items;
	}

	[Serializable]
	internal class StoreShortItem
	{
		public string sku;
		public string name;
		public string description;
		public StoreItemGroup[] groups;
		public StoreItemPromotion[] promotions;
	}
}