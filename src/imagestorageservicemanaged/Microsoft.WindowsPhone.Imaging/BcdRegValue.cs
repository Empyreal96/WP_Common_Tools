using System.Globalization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdRegValue
	{
		private string _name;

		private string _value;

		private string _type;

		public string Name => _name;

		public string Value => _value;

		public string Type => _type;

		public BcdRegValue(string name, string value, string type)
		{
			if (!(type == "REG_BINARY"))
			{
				if (type == "REG_DWORD")
				{
					value = $"0x{value.ToUpper(CultureInfo.InvariantCulture)}";
				}
			}
			else
			{
				value = TrimBinary(value).ToUpper(CultureInfo.InvariantCulture);
			}
			_name = name;
			_value = value;
			_type = type;
		}

		private string TrimBinary(string regBinaryStr)
		{
			return regBinaryStr.Replace("\r\n", "").Replace(",", "").Replace("\\", "")
				.Trim();
		}
	}
}
