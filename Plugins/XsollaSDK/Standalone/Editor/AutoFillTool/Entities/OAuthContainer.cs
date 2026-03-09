#if UNITY_EDITOR
using System;

namespace Xsolla.Core.Editor.AutoFillSettings
{
	[Serializable]
	internal class OAuthContainer
	{
		public int id;
		public bool is_public;
		public string name;
		public string[] redirect_uris;
	}
}
#endif