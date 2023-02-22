using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlRoot(ElementName = "BootConfigurationDatabase", Namespace = "http://schemas.microsoft.com/phone/2011/10/BootConfiguration", IsNullable = false)]
	public class BcdInput
	{
		[XmlAttribute]
		public bool SaveKeyToRegistry { get; set; }

		[XmlAttribute]
		public bool IncludeDescriptions { get; set; }

		[XmlAttribute]
		public bool IncludeRegistryHeader { get; set; }

		public BcdObjectsInput Objects { get; set; }

		private BcdInput()
		{
			SaveKeyToRegistry = true;
		}

		public void SaveAsRegFile(StreamWriter writer, string path)
		{
			Objects.SaveAsRegFile(writer, path);
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			Objects.SaveAsRegData(bcdRegData, path);
		}
	}
}
