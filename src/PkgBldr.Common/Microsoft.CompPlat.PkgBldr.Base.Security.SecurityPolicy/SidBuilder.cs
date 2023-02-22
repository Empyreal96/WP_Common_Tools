using System.Globalization;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public static class SidBuilder
	{
		public static string BuildApplicationSidString(string Name)
		{
			return BuildSidString("S-1-15-2", HashCalculator.CalculateSha256Hash(Name.ToLower(GlobalVariables.Culture)), 7);
		}

		public static string BuildLegacyApplicationSidString(string Name)
		{
			return BuildSidString("S-1-15-2", HashCalculator.CalculateSha256Hash(Name.ToUpper(GlobalVariables.Culture)), 7);
		}

		public static string BuildTaskSidString(string Name)
		{
			return BuildApplicationSidString(Name);
		}

		public static string BuildServiceSidString(string Name)
		{
			return BuildSidString("S-1-5-80", HashCalculator.CalculateSha1Hash(Name.ToUpper(GlobalVariables.Culture)), 5);
		}

		public static string BuildApplicationCapabilitySidString(string CapabilityId)
		{
			string value;
			if (ConstantStrings.LegacyApplicationCapabilityRids.TryGetValue(CapabilityId, out value))
			{
				return "S-1-15-3-" + value;
			}
			return BuildSidString("S-1-15-3-1024", HashCalculator.CalculateSha256Hash(CapabilityId.ToUpper(GlobalVariables.Culture)), 8);
		}

		public static string BuildServiceCapabilitySidString(string CapabilityId)
		{
			return BuildSidString("S-1-5-32", HashCalculator.CalculateSha256Hash(CapabilityId.ToUpper(GlobalVariables.Culture)), 8);
		}

		public static string BuildSidString(string SidPrefix, string HashString, int RidCount)
		{
			StringBuilder stringBuilder = new StringBuilder(SidPrefix);
			if (HashString.Length < RidCount * 8)
			{
				throw new PkgGenException("Insufficient hash bytes to generate SID");
			}
			int num = 0;
			int num2 = 0;
			while (num < HashString.Length && num2 < RidCount)
			{
				stringBuilder.Append('-');
				uint num3 = uint.Parse(HashString.Substring(num, 8), NumberStyles.HexNumber, GlobalVariables.Culture);
				uint value = ((num3 & 0xFF) << 24) | ((num3 & 0xFF00) << 8) | ((num3 & 0xFF0000) >> 8) | ((num3 & 0xFF000000u) >> 24);
				stringBuilder.Append(value);
				num += 8;
				num2++;
			}
			return stringBuilder.ToString();
		}
	}
}
