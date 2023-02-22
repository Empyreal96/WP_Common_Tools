using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Phone.Test.TestMetadata.ObjectModel;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	[Serializable]
	[XmlRoot(ElementName = "Required")]
	public class ResolvedDependency
	{
		private HashSet<Dependency> _dependencies = new HashSet<Dependency>();

		[XmlElement("Package")]
		public List<PackageDependency> PackageDependency => _dependencies.OfType<PackageDependency>().ToList();

		[XmlElement("RemoteFile")]
		public List<RemoteFileDependency> RemoteFileDependency => _dependencies.OfType<RemoteFileDependency>().ToList();

		[XmlElement("EnvironmentPath")]
		public List<EnvironmentPathDependnecy> EnvironmentPaths => _dependencies.OfType<EnvironmentPathDependnecy>().ToList();

		internal static void Save(string fileName, HashSet<Dependency> dependencies)
		{
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true
			};
			ResolvedDependency o = new ResolvedDependency
			{
				_dependencies = dependencies
			};
			using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
			{
				new XmlSerializer(typeof(ResolvedDependency)).Serialize(xmlWriter, o);
			}
		}
	}
}
