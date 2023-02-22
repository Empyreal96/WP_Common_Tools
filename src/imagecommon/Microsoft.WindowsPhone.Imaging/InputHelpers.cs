using System;
using System.Globalization;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class InputHelpers
	{
		public static bool StringToUint(string valueAsString, out uint value)
		{
			bool result = true;
			if (valueAsString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				if (!uint.TryParse(valueAsString.Substring(2, valueAsString.Length - 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
				{
					result = false;
				}
			}
			else if (!uint.TryParse(valueAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
			{
				result = false;
			}
			return result;
		}

		public static bool IsPowerOfTwo(uint value)
		{
			return (value & (value - 1)) == 0;
		}
	}
}
