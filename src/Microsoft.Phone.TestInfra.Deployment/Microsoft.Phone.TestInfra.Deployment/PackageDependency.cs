using System.Xml.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageDependency : Dependency
	{
		[XmlIgnore]
		public string PkgName { get; set; }

		[XmlAttribute(AttributeName = "Name")]
		public string RelativePath { get; set; }

		[XmlIgnore]
		public string AbsolutePath { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals(obj as PackageDependency);
		}

		public bool Equals(PackageDependency other)
		{
			return string.Compare(PkgName, other.PkgName, true) == 0;
		}

		public override int GetHashCode()
		{
			return PkgName.ToLowerInvariant().GetHashCode();
		}
	}
}
