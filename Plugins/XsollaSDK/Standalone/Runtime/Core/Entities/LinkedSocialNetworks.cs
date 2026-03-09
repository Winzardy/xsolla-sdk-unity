using System;
using System.Collections.Generic;

namespace Xsolla.UserAccount
{
	[Serializable]
	internal class LinkedSocialNetworks
	{
		public List<LinkedSocialNetwork> items;
	}
	
	[Serializable]
	internal class LinkedSocialNetwork
	{
		public string full_name;
		public string nickname;
		public string picture;
		public string provider;
		public string social_id;
	}
}