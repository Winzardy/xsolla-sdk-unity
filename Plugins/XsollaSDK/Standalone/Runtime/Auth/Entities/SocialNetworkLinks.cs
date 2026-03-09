using System;
using System.Collections.Generic;

namespace Xsolla.Auth
{
	[Serializable]
	internal class SocialNetworkLinks
	{
		public List<SocialNetworkLink> items;
	}

	[Serializable]
	internal class SocialNetworkLink
	{
		// Link for authentication via the social network.
		public string auth_url;

		// Name of the social network.
		public string provider;
	}
}