using System;

namespace Xsolla.Core
{
	[Serializable]
	internal class StoreItemAttribute
	{
		public string external_id;
		public string name;
		public ValuePair[] values;

		[Serializable]
		public class ValuePair
		{
			public string external_id;
			public string value;
		}
	}
}