using System.Xml.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class EnvironmentPathDependency : Dependency
	{
		[XmlAttribute(AttributeName = "Name")]
		public string EnvironmentPath { get; set; }

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
			return Equals(obj as EnvironmentPathDependency);
		}

		public bool Equals(EnvironmentPathDependency other)
		{
			return string.Compare(EnvironmentPath, other.EnvironmentPath, true) == 0;
		}

		public override int GetHashCode()
		{
			return EnvironmentPath.ToLowerInvariant().GetHashCode();
		}
	}
}
