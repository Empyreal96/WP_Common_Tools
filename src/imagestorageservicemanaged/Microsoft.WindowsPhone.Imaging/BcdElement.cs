using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Win32;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElement
	{
		private byte[] _binaryData;

		private string _stringData;

		protected List<string> _multiStringData;

		public BcdElementDataType DataType { get; set; }

		public string StringData
		{
			get
			{
				return _stringData;
			}
			set
			{
				if (DataType.RegistryValueType != RegistryValueKind.String)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Cannot set string data for an element format: {DataType.Format}");
				}
				_stringData = value;
			}
		}

		public List<string> MultiStringData => _multiStringData;

		public override string ToString()
		{
			if (DataType != null)
			{
				return DataType.ToString();
			}
			return "Unnamed BcdElement";
		}

		public static BcdElement CreateElement(OfflineRegistryHandle elementKey)
		{
			BcdElement bcdElement = null;
			BcdElementDataType bcdElementDataType = new BcdElementDataType();
			byte[] binaryData = null;
			string text = null;
			string[] multiStringData = null;
			uint rawValue = uint.Parse(elementKey.Name.Substring(elementKey.Name.LastIndexOf('\\') + 1), NumberStyles.HexNumber);
			bcdElementDataType.RawValue = rawValue;
			switch (bcdElementDataType.RegistryValueType)
			{
			case RegistryValueKind.Binary:
				binaryData = (byte[])elementKey.GetValue("Element", null);
				break;
			case RegistryValueKind.String:
				text = (string)elementKey.GetValue("Element", string.Empty);
				break;
			case RegistryValueKind.MultiString:
				multiStringData = (string[])elementKey.GetValue("Element", null);
				break;
			default:
				return null;
			}
			switch (bcdElementDataType.Format)
			{
			case ElementFormat.Boolean:
				return new BcdElementBoolean(binaryData, bcdElementDataType);
			case ElementFormat.Device:
				return new BcdElementDevice(binaryData, bcdElementDataType);
			case ElementFormat.Integer:
				return new BcdElementInteger(binaryData, bcdElementDataType);
			case ElementFormat.IntegerList:
				return new BcdElementIntegerList(binaryData, bcdElementDataType);
			case ElementFormat.Object:
				return new BcdElementObject(text, bcdElementDataType);
			case ElementFormat.ObjectList:
				return new BcdElementObjectList(multiStringData, bcdElementDataType);
			case ElementFormat.String:
				return new BcdElementString(text, bcdElementDataType);
			default:
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unknown element format: {bcdElementDataType.RawValue}.");
			}
		}

		protected BcdElement(BcdElementDataType dataType)
		{
			DataType = dataType;
		}

		public byte[] GetBinaryData()
		{
			return _binaryData;
		}

		public void SetBinaryData(byte[] binaryData)
		{
			if (DataType.RegistryValueType != RegistryValueKind.Binary)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Cannot set binary data for an element format: {DataType.Format}");
			}
			_binaryData = binaryData;
		}

		[CLSCompliant(false)]
		public virtual void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "BCD Element:        {0:x}", DataType.RawValue);
			DataType.LogInfo(logger, indentLevel);
		}
	}
}
