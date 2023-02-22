using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	[Serializable]
	public class PackageDescription
	{
		[XmlAttribute(AttributeName = "Path")]
		public string RelativePath { get; set; }

		[XmlElement]
		public List<Dependency> Dependencies { get; set; }

		public List<string> Binaries { get; set; }

		public PackageDescription()
		{
			RelativePath = string.Empty;
			Dependencies = new List<Dependency>();
			Binaries = new List<string>();
		}
	}
}
