using System;
using System.Collections.Generic;

namespace Xsolla.Catalog
{
	[Serializable]
	internal class Groups
	{
		public Group[] groups;
	}

	[Serializable]
	internal class Group
	{
		public string external_id;
		public string name;
		public string description;
		public string image_url;
		public int level;
		public int? order;
		public List<Group> children;
		public string parent_external_id;
	}
}