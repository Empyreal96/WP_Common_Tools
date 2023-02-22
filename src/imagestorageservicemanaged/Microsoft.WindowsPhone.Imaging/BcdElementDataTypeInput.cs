using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementDataTypeInput
	{
		[XmlChoiceIdentifier("TypeIdentifier")]
		[XmlElement("WellKnownType", typeof(string))]
		[XmlElement("RawType", typeof(string))]
		public object DataType { get; set; }

		[XmlIgnore]
		public DataTypeChoice TypeIdentifier { get; set; }

		[XmlIgnore]
		public BcdElementDataType Type
		{
			get
			{
				if (TypeIdentifier != 0)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Only WellKnownTypes are currently supported.");
				}
				BcdElementDataType wellKnownDataType = BcdElementDataTypes.GetWellKnownDataType(DataType as string);
				if (wellKnownDataType == null)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The element for well known type '{DataType as string}' cannot be translated.");
				}
				return wellKnownDataType;
			}
		}

		public void SaveAsRegFile(TextWriter writer, string path)
		{
			writer.WriteLine("[{0}\\{1:x8}]", path, Type.RawValue);
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			bcdRegData.AddRegKey($"{path}\\{Type.RawValue:x8}");
		}
	}
}
