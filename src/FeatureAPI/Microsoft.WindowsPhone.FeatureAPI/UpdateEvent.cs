using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[XmlRoot(ElementName = "UpdateEvent", IsNullable = false)]
	public class UpdateEvent
	{
		public int Sequence;

		public string DateTime;

		public string Summary;

		[XmlElement("UpdateOSOutput")]
		public UpdateOSOutput UpdateResults;
	}
}
