using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementIntegerListInput
	{
		[XmlArrayItem(ElementName = "StringValue", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public string[] StringValues { get; set; }

		public void SaveAsRegFile(StreamWriter writer, string elementName)
		{
			writer.Write("\"Element\"=hex:");
			for (int i = 0; i < StringValues.Length; i++)
			{
				BcdElementValueTypeInput.WriteIntegerValue(writer, elementName, StringValues[i]);
				if (i < StringValues.Length - 1)
				{
					writer.Write(",");
				}
			}
			writer.WriteLine();
			writer.WriteLine();
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			for (int i = 0; i < StringValues.Length; i++)
			{
				BcdElementValueTypeInput.WriteIntegerValue(streamWriter, "", StringValues[i]);
			}
			streamWriter.Flush();
			memoryStream.Position = 0L;
			string value = new StreamReader(memoryStream).ReadToEnd();
			bcdRegData.AddRegValue(path, "Element", value, "REG_BINARY");
		}
	}
}
