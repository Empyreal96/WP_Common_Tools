using System.Xml.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class BinaryDependency : Dependency
	{
		[XmlAttribute(AttributeName = "Name")]
		public string FileName { get; set; }

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
			return Equals(obj as BinaryDependency);
		}

		public bool Equals(BinaryDependency other)
		{
			return string.Compare(FileName, other.FileName, true) == 0;
		}

		public override int GetHashCode()
		{
			return FileName.GetHashCode();
		}
	}
}
