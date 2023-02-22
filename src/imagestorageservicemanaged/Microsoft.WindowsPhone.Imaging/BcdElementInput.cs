using System.IO;
using System.Reflection;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementInput
	{
		public BcdElementDataTypeInput DataType { get; set; }

		public BcdElementValueTypeInput ValueType { get; set; }

		protected void RegFilePreProcessing()
		{
			if (DataType.TypeIdentifier != 0 || BcdElementDataTypes.GetWellKnownDataType(DataType.DataType as string) != BcdElementDataTypes.CustomActionsList)
			{
				return;
			}
			if (ValueType.ValueIdentifier != ValueTypeChoice.IntegerListValue)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: A custom action list should have an integer list associated with it.");
			}
			BcdElementIntegerListInput bcdElementIntegerListInput = ValueType.ValueType as BcdElementIntegerListInput;
			if (bcdElementIntegerListInput.StringValues.Length % 2 != 0)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: A custom action list should have one element associated with each scan key code.");
			}
			for (int i = 0; i < bcdElementIntegerListInput.StringValues.Length; i += 2)
			{
				BcdElementDataType wellKnownDataType = BcdElementDataTypes.GetWellKnownDataType(bcdElementIntegerListInput.StringValues[i + 1]);
				if (wellKnownDataType != null)
				{
					bcdElementIntegerListInput.StringValues[i + 1] = $"{wellKnownDataType.RawValue:x8}";
				}
			}
		}

		public void SaveAsRegFile(StreamWriter writer, string path)
		{
			RegFilePreProcessing();
			DataType.SaveAsRegFile(writer, path);
			ValueType.SaveAsRegFile(writer, $"{DataType.Type:x8}");
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			RegFilePreProcessing();
			DataType.SaveAsRegData(bcdRegData, path);
			ValueType.SaveAsRegData(bcdRegData, $"{path}\\{DataType.Type:x8}");
		}
	}
}
