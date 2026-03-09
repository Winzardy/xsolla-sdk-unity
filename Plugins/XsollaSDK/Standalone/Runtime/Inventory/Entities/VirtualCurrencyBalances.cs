using System;

namespace Xsolla.Inventory
{
	[Serializable]
	internal class VirtualCurrencyBalances
	{
		public VirtualCurrencyBalance[] items;
	}

	[Serializable]
	internal class VirtualCurrencyBalance
	{
		public string sku;
		public string type;
		public string name;
		public int amount;
		public string description;
		public string image_url;
	}
}