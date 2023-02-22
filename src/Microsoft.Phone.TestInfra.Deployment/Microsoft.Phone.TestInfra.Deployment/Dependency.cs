using System.Xml.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	[XmlInclude(typeof(BinaryDependency))]
	[XmlInclude(typeof(PackageDependency))]
	[XmlInclude(typeof(EnvironmentPathDependency))]
	[XmlInclude(typeof(RemoteFileDependency))]
	public abstract class Dependency
	{
	}
}
