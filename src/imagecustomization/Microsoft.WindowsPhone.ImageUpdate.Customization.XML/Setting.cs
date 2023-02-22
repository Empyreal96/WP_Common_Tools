using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Setting : IDefinedIn
	{
		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Value { get; set; }

		[XmlAttribute]
		public string Type { get; set; }
	}
}
