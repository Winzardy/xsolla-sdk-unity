using System;

namespace Xsolla.Inventory
{
	[Serializable]
	internal class ConsumeItem
	{
		public string sku;
		public int? quantity;
		public string instance_id;
	}
}