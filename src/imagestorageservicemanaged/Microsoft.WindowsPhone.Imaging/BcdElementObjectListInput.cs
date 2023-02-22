using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementObjectListInput
	{
		[XmlArrayItem(ElementName = "StringValue", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public string[] StringValues { get; set; }

		public void SaveAsRegFile(TextWriter writer, string elementName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < StringValues.Length; i++)
			{
				Guid guid = BcdObjects.IdFromName(StringValues[i]);
				stringBuilder.Append($"{{{guid}}}");
				stringBuilder.Append("\0");
			}
			stringBuilder.Append("\0");
			BcdElementValueTypeInput.WriteObjectsValue(writer, elementName, "\"Element\"=hex(7):", stringBuilder.ToString());
			string[] stringValues = StringValues;
			foreach (string text in stringValues)
			{
				Guid guid2 = BcdObjects.IdFromName(text);
				writer.WriteLine(";Values={{{0}}}, \"{1}\"", guid2, text);
			}
			writer.WriteLine();
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			string text = null;
			string[] stringValues = StringValues;
			foreach (string objectName in stringValues)
			{
				text += "\"{";
				Guid guid = BcdObjects.IdFromName(objectName);
				text = string.Concat(text, guid, "}\",");
			}
			text = text.TrimEnd(',');
			bcdRegData.AddRegValue(path, "Element", text, "REG_MULTI_SZ");
		}
	}
}
