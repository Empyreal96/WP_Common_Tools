using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType("Objects")]
	public class BcdObjectsInput
	{
		[XmlElement("Object")]
		public BcdObjectInput[] Objects { get; set; }

		[XmlAttribute]
		public bool SaveKeyToRegistry { get; set; }

		private BcdObjectsInput()
		{
			SaveKeyToRegistry = true;
		}

		public void SaveAsRegFile(StreamWriter writer, string path)
		{
			string text = path + "\\Objects";
			if (SaveKeyToRegistry)
			{
				writer.WriteLine("[{0}]", text);
				writer.WriteLine();
			}
			BcdObjectInput[] objects = Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].SaveAsRegFile(writer, text);
			}
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			string text = path + "\\Objects";
			if (SaveKeyToRegistry)
			{
				bcdRegData.AddRegKey(text);
			}
			BcdObjectInput[] objects = Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].SaveAsRegData(bcdRegData, text);
			}
		}
	}
}
