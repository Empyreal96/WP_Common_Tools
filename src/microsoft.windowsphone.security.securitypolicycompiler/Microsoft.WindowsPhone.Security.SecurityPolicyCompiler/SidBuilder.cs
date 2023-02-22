using System.Globalization;
using System.Text;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public static class SidBuilder
	{
		public static string BuildApplicationSidString(string name)
		{
			return BuildSidString("S-1-15-2", HashCalculator.CalculateSha256Hash(name, true), 7);
		}

		public static string BuildServiceSidString(string name)
		{
			return BuildSidString("S-1-5-80", HashCalculator.CalculateSha1Hash(name.ToUpper(GlobalVariables.Culture)), 8);
		}

		public static string BuildApplicationCapabilitySidString(string capabilityId)
		{
			return BuildSidString("S-1-15-3-1024", HashCalculator.CalculateSha256Hash(capabilityId, true), 8);
		}

		public static string BuildSidString(string sidPrefix, string sidHash, int numberOfRids)
		{
			if (string.IsNullOrEmpty(sidPrefix) || string.IsNullOrEmpty(sidHash))
			{
				throw new PolicyCompilerInternalException("Invalid arguments");
			}
			if (sidHash.Length % 8 != 0)
			{
				throw new PolicyCompilerInternalException("sidHash length is not divisible by 8.");
			}
			StringBuilder stringBuilder = new StringBuilder(sidPrefix);
			int num = 0;
			int num2 = 0;
			while (num < sidHash.Length && num2 < numberOfRids)
			{
				stringBuilder.Append('-');
				string empty = string.Empty;
				uint num3 = uint.Parse(sidHash.Substring(num, 8), NumberStyles.HexNumber, GlobalVariables.Culture);
				uint value = ((num3 & 0xFF) << 24) | ((num3 & 0xFF00) << 8) | ((num3 & 0xFF0000) >> 8) | ((num3 & 0xFF000000u) >> 24);
				stringBuilder.Append(value);
				num += 8;
				num2++;
			}
			return stringBuilder.ToString();
		}
	}
}
