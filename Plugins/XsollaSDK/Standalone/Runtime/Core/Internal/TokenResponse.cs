using System;

namespace Xsolla.Core
{
	[Serializable]
	internal class TokenResponse
	{
		public string access_token;
		public string token_type;
		public int expires_in;
		public string refresh_token;
		public string scope;
	}
	
	[Serializable]
	internal class TokenResponseSimple
	{
		public string token;
	}
}