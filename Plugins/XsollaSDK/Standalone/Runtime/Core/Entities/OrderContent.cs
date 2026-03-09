using System;

namespace Xsolla.Core
{
	[Serializable]
	internal class OrderContent
	{
		public Price price;
		public VirtualPrice virtual_price;
		public string is_free;
		public OrderItem[] items;
	}
}