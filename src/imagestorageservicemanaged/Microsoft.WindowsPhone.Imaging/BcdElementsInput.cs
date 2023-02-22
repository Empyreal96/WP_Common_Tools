using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType("Elements")]
	public class BcdElementsInput
	{
		[XmlElement("Element")]
		public BcdElementInput[] Elements { get; set; }

		[XmlAttribute]
		public bool SaveKeyToRegistry { get; set; }

		private BcdElementsInput()
		{
			SaveKeyToRegistry = true;
		}

		public void SaveAsRegFile(StreamWriter writer, string path)
		{
			if (SaveKeyToRegistry)
			{
				writer.WriteLine("[{0}\\Elements]", path);
				writer.WriteLine();
			}
			BcdElementInput[] elements = Elements;
			for (int i = 0; i < elements.Length; i++)
			{
				elements[i].SaveAsRegFile(writer, path + "\\Elements");
			}
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			if (SaveKeyToRegistry)
			{
				bcdRegData.AddRegKey(path);
			}
			string path2 = $"{path}\\Elements";
			BcdElementInput[] elements = Elements;
			for (int i = 0; i < elements.Length; i++)
			{
				elements[i].SaveAsRegData(bcdRegData, path2);
			}
		}
	}
}
