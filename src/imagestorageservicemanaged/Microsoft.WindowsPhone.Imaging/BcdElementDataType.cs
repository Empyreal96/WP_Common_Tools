using System;
using System.Text;
using Microsoft.Win32;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementDataType
	{
		[CLSCompliant(false)]
		public uint RawValue { get; set; }

		[CLSCompliant(false)]
		public ElementClass Class
		{
			get
			{
				return (ElementClass)((RawValue & 0xF0000000u) >> 28);
			}
			set
			{
				RawValue = (RawValue & 0xFFFFFFFu) | ((uint)value << 28);
			}
		}

		[CLSCompliant(false)]
		public ElementFormat Format
		{
			get
			{
				return (ElementFormat)((RawValue & 0xF000000) >> 24);
			}
			set
			{
				RawValue = (RawValue & 0xF0FFFFFFu) | ((uint)value << 24);
			}
		}

		[CLSCompliant(false)]
		public uint SubClass
		{
			get
			{
				return RawValue & 0xFFFFFFu;
			}
			set
			{
				RawValue = (RawValue & 0xFF000000u) | (value & 0xFFFFFFu);
			}
		}

		public RegistryValueKind RegistryValueType
		{
			get
			{
				switch (Format)
				{
				case ElementFormat.Boolean:
					return RegistryValueKind.Binary;
				case ElementFormat.Device:
					return RegistryValueKind.Binary;
				case ElementFormat.Integer:
					return RegistryValueKind.Binary;
				case ElementFormat.IntegerList:
					return RegistryValueKind.Binary;
				case ElementFormat.Object:
					return RegistryValueKind.String;
				case ElementFormat.ObjectList:
					return RegistryValueKind.MultiString;
				case ElementFormat.String:
					return RegistryValueKind.String;
				default:
					return RegistryValueKind.Binary;
				}
			}
		}

		public BcdElementDataType()
		{
		}

		[CLSCompliant(false)]
		public BcdElementDataType(uint dataType)
		{
			RawValue = dataType;
		}

		[CLSCompliant(false)]
		public BcdElementDataType(ElementClass elementClass, ElementFormat elementFormat, uint elementSubClass)
		{
			Class = elementClass;
			Format = elementFormat;
			SubClass = elementSubClass;
		}

		public override bool Equals(object obj)
		{
			BcdElementDataType bcdElementDataType = obj as BcdElementDataType;
			if (bcdElementDataType == null)
			{
				return false;
			}
			if (bcdElementDataType.Format == Format && bcdElementDataType.Class == Class && bcdElementDataType.SubClass == SubClass)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)Format ^ (int)Class ^ (int)SubClass;
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Class:              {0}", Class);
			logger.LogInfo(text + "Format:             {0}", Format);
			if (BcdElementDataTypes.ApplicationTypes.ContainsKey(this))
			{
				logger.LogInfo(text + "SubClass:           {0} (0x{1:x})", BcdElementDataTypes.ApplicationTypes[this], SubClass);
			}
			else if (BcdElementDataTypes.LibraryTypes.ContainsKey(this))
			{
				logger.LogInfo(text + "SubClass:           {0} (0x{1:x})", BcdElementDataTypes.LibraryTypes[this], SubClass);
			}
			else if (BcdElementDataTypes.DeviceTypes.ContainsKey(this))
			{
				logger.LogInfo(text + "SubClass:           {0} (0x{1:x})", BcdElementDataTypes.DeviceTypes[this], SubClass);
			}
			else
			{
				logger.LogInfo(text + "SubClass:           0x{0:x}", SubClass);
			}
			logger.LogInfo(text + "Registry Data Type: {0}", RegistryValueType);
		}

		public override string ToString()
		{
			return $"{RawValue:x8}";
		}
	}
}
