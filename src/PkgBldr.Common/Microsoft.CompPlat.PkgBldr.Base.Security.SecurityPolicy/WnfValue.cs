namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public class WnfValue
	{
		public string Name { get; set; }

		public string Tag { get; set; }

		public string Scope { get; set; }

		public string Sequence { get; set; }

		public string SecurityDescriptor { get; set; }

		public WnfValue(string WnfName, string WnfTag, string WnfScope, string WnfSequence)
		{
			Name = WnfName;
			Tag = WnfTag;
			Scope = WnfScope;
			Sequence = WnfSequence;
		}

		public string GetId()
		{
			return Tag + Scope + Sequence;
		}
	}
}
