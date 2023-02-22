using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Target : IDefinedIn
	{
		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Id { get; set; }

		[XmlElement(ElementName = "TargetState")]
		public List<TargetState> TargetStates { get; set; }

		public Target()
		{
			TargetStates = new List<TargetState>();
		}

		public Target(string id)
			: this()
		{
			Id = id;
		}
	}
}
