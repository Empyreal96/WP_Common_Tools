using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	[Serializable]
	[XmlRoot(ElementName = "Required")]
	public class ResolvedDependency
	{
		[XmlIgnore]
		public HashSet<Dependency> Dependencies { get; set; }

		[XmlElement("Binary")]
		public List<BinaryDependency> BinaryDependency => Dependencies.OfType<BinaryDependency>().ToList();

		[XmlElement("Package")]
		public List<PackageDependency> PackageDependency => Dependencies.OfType<PackageDependency>().ToList();

		[XmlElement("RemoteFile")]
		public List<RemoteFileDependency> RemoteFileDependency => Dependencies.OfType<RemoteFileDependency>().ToList();

		[XmlElement("EnvironmentPath")]
		public List<EnvironmentPathDependency> EnvironmentPaths => Dependencies.OfType<EnvironmentPathDependency>().ToList();

		internal static void Save(string fileName, HashSet<Dependency> dependencies)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			XmlWriterSettings settings = xmlWriterSettings;
			ResolvedDependency resolvedDependency = new ResolvedDependency();
			resolvedDependency.Dependencies = dependencies;
			ResolvedDependency o = resolvedDependency;
			using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResolvedDependency));
				xmlSerializer.Serialize(xmlWriter, o);
			}
		}
	}
}
