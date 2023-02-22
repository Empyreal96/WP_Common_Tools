using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.CompDB
{
	[Serializable]
	[XmlRoot(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v3")]
	public class AssemblyIdentity
	{
		[XmlAttribute]
		public string buildType;

		[XmlAttribute]
		public string language;

		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public string processorArchitecture;

		[XmlAttribute]
		public string publicKeyToken;

		[XmlAttribute]
		public string version;
	}
}
