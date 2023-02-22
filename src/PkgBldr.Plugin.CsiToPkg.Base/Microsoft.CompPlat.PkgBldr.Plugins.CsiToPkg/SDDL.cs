namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	internal class SDDL
	{
		private string owner;

		private string group;

		private string dacl;

		private string sacl;

		public string Owner
		{
			get
			{
				return owner;
			}
			set
			{
				owner = value;
			}
		}

		public string Group
		{
			get
			{
				return group;
			}
			set
			{
				group = value;
			}
		}

		public string Dacl
		{
			get
			{
				return dacl;
			}
			set
			{
				dacl = value;
			}
		}

		public string Sacl
		{
			get
			{
				return sacl;
			}
			set
			{
				sacl = value;
			}
		}
	}
}
