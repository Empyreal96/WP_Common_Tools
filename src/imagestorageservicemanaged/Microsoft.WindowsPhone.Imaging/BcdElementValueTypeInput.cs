using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementValueTypeInput
	{
		[XmlChoiceIdentifier("ValueIdentifier")]
		[XmlElement("StringValue", typeof(string))]
		[XmlElement("BooleanValue", typeof(bool))]
		[XmlElement("ObjectValue", typeof(string))]
		[XmlElement("ObjectListValue", typeof(BcdElementObjectListInput))]
		[XmlElement("IntegerValue", typeof(string))]
		[XmlElement("IntegerListValue", typeof(BcdElementIntegerListInput))]
		[XmlElement("DeviceValue", typeof(BcdElementDeviceInput))]
		public object ValueType { get; set; }

		[XmlIgnore]
		public ValueTypeChoice ValueIdentifier { get; set; }

		private static bool StringToUlong(string valueAsString, out ulong value)
		{
			bool result = true;
			int num = 0;
			if (valueAsString.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
			{
				num = 2;
			}
			if (!ulong.TryParse(valueAsString.Substring(num, valueAsString.Length - num), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
			{
				result = false;
			}
			return result;
		}

		public static void WriteIntegerValue(StreamWriter writer, string elementName, string valueAsString)
		{
			ulong value = 0uL;
			if (!StringToUlong(valueAsString, out value))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unable to parse value for element '{elementName}'.");
			}
			byte[] array = new byte[8];
			MemoryStream memoryStream = null;
			BinaryWriter binaryWriter = null;
			try
			{
				memoryStream = new MemoryStream(array);
				binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(value);
				for (int i = 0; i < array.Length; i++)
				{
					writer.Write("{0:x2}{1}", array[i], (i < array.Length - 1) ? "," : "");
				}
			}
			finally
			{
				if (binaryWriter != null)
				{
					binaryWriter.Flush();
					binaryWriter = null;
				}
				if (memoryStream != null)
				{
					memoryStream.Flush();
					memoryStream.Close();
					memoryStream = null;
				}
			}
		}

		public void WriteIntegerValue(BcdRegData bcdRegData, string path, string valueAsString)
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			WriteIntegerValue(streamWriter, "", valueAsString);
			streamWriter.Flush();
			memoryStream.Position = 0L;
			string value = new StreamReader(memoryStream).ReadToEnd();
			bcdRegData.AddRegValue(path, "Element", value, "REG_BINARY");
		}

		public static void WriteByteArray(TextWriter writer, string elementName, string elementHeader, byte[] value)
		{
			writer.Write(elementHeader);
			int i = elementHeader.Length;
			int num = 0;
			while (num < value.Length - 1)
			{
				for (; i < 80; i += 3)
				{
					if (num >= value.Length - 1)
					{
						break;
					}
					writer.Write("{0:x2},", value[num++]);
				}
				if (i >= 80)
				{
					if (num < value.Length - 1)
					{
						writer.WriteLine("\\");
					}
					i = 0;
				}
			}
			writer.WriteLine("{0:x2}", value[value.Length - 1]);
		}

		public static void WriteByteArray(BcdRegData bcdRegData, string path, byte[] value)
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			WriteByteArray(streamWriter, "", "", value);
			streamWriter.Flush();
			memoryStream.Position = 0L;
			string value2 = new StreamReader(memoryStream).ReadToEnd();
			bcdRegData.AddRegValue(path, "Element", value2, "REG_BINARY");
		}

		public static void WriteObjectsValue(TextWriter writer, string elementName, string elementHeader, string objectsAsStrings)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(objectsAsStrings);
			WriteByteArray(writer, elementName, elementHeader, bytes);
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			switch (ValueIdentifier)
			{
			case ValueTypeChoice.BooleanValue:
				bcdRegData.AddRegValue(path, "Element", string.Format("{0}", ((bool)ValueType) ? "01" : "00"), "REG_BINARY");
				break;
			case ValueTypeChoice.DeviceValue:
				(ValueType as BcdElementDeviceInput).SaveAsRegData(bcdRegData, path);
				break;
			case ValueTypeChoice.IntegerListValue:
				(ValueType as BcdElementIntegerListInput).SaveAsRegData(bcdRegData, path);
				break;
			case ValueTypeChoice.IntegerValue:
				WriteIntegerValue(bcdRegData, path, ValueType as string);
				break;
			case ValueTypeChoice.ObjectListValue:
				(ValueType as BcdElementObjectListInput).SaveAsRegData(bcdRegData, path);
				break;
			case ValueTypeChoice.ObjectValue:
			{
				string value2 = $"{{{BcdObjects.IdFromName(ValueType as string).ToString()}}}";
				bcdRegData.AddRegValue(path, "Element", value2, "REG_SZ");
				break;
			}
			case ValueTypeChoice.StringValue:
			{
				string value = new StringBuilder(ValueType as string).ToString();
				bcdRegData.AddRegValue(path, "Element", value, "REG_SZ");
				break;
			}
			default:
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Invalid value type for element '{path}'.");
			}
		}

		public void SaveAsRegFile(StreamWriter writer, string elementName)
		{
			switch (ValueIdentifier)
			{
			case ValueTypeChoice.BooleanValue:
				writer.WriteLine("\"Element\"=hex:{0}", ((bool)ValueType) ? "01" : "00");
				writer.WriteLine();
				break;
			case ValueTypeChoice.DeviceValue:
				(ValueType as BcdElementDeviceInput).SaveAsRegFile(writer, elementName);
				break;
			case ValueTypeChoice.IntegerListValue:
				(ValueType as BcdElementIntegerListInput).SaveAsRegFile(writer, elementName);
				break;
			case ValueTypeChoice.IntegerValue:
				writer.Write("\"Element\"=hex:");
				WriteIntegerValue(writer, elementName, ValueType as string);
				writer.WriteLine();
				writer.WriteLine();
				break;
			case ValueTypeChoice.ObjectListValue:
				(ValueType as BcdElementObjectListInput).SaveAsRegFile(writer, elementName);
				break;
			case ValueTypeChoice.ObjectValue:
			{
				string arg = $"\"Element\"=\"{{{BcdObjects.IdFromName(ValueType as string).ToString()}}}\"";
				writer.WriteLine("{0}", arg);
				writer.WriteLine();
				writer.Flush();
				break;
			}
			case ValueTypeChoice.StringValue:
			{
				StringBuilder stringBuilder = new StringBuilder(ValueType as string);
				for (int i = 0; i < stringBuilder.Length; i++)
				{
					if (stringBuilder[i] == '\\')
					{
						stringBuilder.Insert(i++, '\\');
					}
				}
				writer.WriteLine("\"Element\"=\"{0}\"", stringBuilder.ToString());
				writer.WriteLine();
				break;
			}
			default:
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Invalid value type for element '{elementName}'.");
			}
		}
	}
}
