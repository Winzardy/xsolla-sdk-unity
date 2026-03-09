using JetBrains.Annotations;
using System;
using System.Globalization;
using Xsolla.Core;

namespace Xsolla.Catalog
{
	[Serializable]
	internal class StoreItems
	{
		public bool has_more;
		public StoreItem[] items;

    [NonSerialized, CanBeNull]
    internal CultureInfo geoLocale = null;
	}
}