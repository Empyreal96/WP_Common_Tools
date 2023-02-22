using System;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdObjectInput
	{
		public string FriendlyName { get; set; }

		public int RawType { get; set; }

		[XmlElement("Id")]
		public string IdAsString { get; set; }

		public BcdElementsInput Elements { get; set; }

		[CLSCompliant(false)]
		[XmlIgnore]
		public uint ObjectType => BcdObjects.ObjectTypeFromName(FriendlyName);

		[XmlIgnore]
		public Guid Id => BcdObjects.IdFromName(FriendlyName);

		[XmlAttribute]
		public bool SaveKeyToRegistry { get; set; }

		private BcdObjectInput()
		{
			SaveKeyToRegistry = true;
		}

		public void SaveAsRegFile(StreamWriter writer, string path)
		{
			string text = $"{path}\\{{{Id}}}";
			if (SaveKeyToRegistry)
			{
				writer.WriteLine("[{0}]", text);
				writer.WriteLine();
				writer.WriteLine("[{0}\\Description]", text);
				writer.WriteLine("\"Type\"=dword:{0:x8}", ObjectType);
				writer.WriteLine();
			}
			Elements.SaveAsRegFile(writer, text);
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			string text = $"{path}\\{{{Id}}}";
			if (SaveKeyToRegistry)
			{
				string regKey = $"{text}\\Description";
				bcdRegData.AddRegKey(text);
				bcdRegData.AddRegKey(regKey);
				bcdRegData.AddRegValue(regKey, "Type", $"{ObjectType:x8}", "REG_DWORD");
			}
			Elements.SaveAsRegData(bcdRegData, text);
		}

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(FriendlyName))
			{
				return FriendlyName;
			}
			return base.ToString();
		}
	}
}
