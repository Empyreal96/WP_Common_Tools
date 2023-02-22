using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class TargetState
	{
		[XmlElement(ElementName = "Condition")]
		public List<Condition> Items { get; set; }

		public TargetState()
		{
			Items = new List<Condition>();
		}
	}
}
