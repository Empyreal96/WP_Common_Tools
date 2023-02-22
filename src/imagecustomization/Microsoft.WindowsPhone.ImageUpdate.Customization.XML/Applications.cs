using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Applications
	{
		[XmlElement(ElementName = "Application")]
		public List<Application> Items { get; set; }

		public Applications()
		{
			Items = new List<Application>();
		}
	}
}
