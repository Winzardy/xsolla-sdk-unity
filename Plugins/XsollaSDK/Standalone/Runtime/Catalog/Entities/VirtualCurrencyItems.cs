using System;
using Xsolla.Core;

namespace Xsolla.Catalog
{
	[Serializable]
	internal class VirtualCurrencyItems
	{
		public bool has_more;
		public VirtualCurrencyItem[] items;
	}

	[Serializable]
	internal class VirtualCurrencyItem : StoreItem { }
}