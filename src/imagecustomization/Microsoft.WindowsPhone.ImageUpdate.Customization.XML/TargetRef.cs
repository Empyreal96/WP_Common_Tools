using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class TargetRef : IDefinedIn
	{
		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Id { get; set; }

		public TargetRef()
		{
		}

		public TargetRef(string id)
		{
			Id = id;
		}
	}
}
