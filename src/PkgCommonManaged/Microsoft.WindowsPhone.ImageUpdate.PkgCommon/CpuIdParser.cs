using System.Globalization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public static class CpuIdParser
	{
		private static StringToEnum<CpuId> parser;

		static CpuIdParser()
		{
			parser = new StringToEnum<CpuId>();
			parser.Add(CpuId.X86, "X86");
			parser.Add(CpuId.ARM, "ARM");
			parser.Add(CpuId.ARM64, "ARM64");
			parser.Add(CpuId.AMD64, "AMD64");
			parser.Add(CpuId.AMD64_X86, "WOW64");
			parser.Add(CpuId.AMD64_X86, "AMD64.X86");
			parser.Add(CpuId.ARM64_ARM, "ARM64.ARM");
			parser.Add(CpuId.ARM64_X86, "ARM64.X86");
			parser.Add(CpuId.AMD64_X86, "AMD64_X86");
			parser.Add(CpuId.ARM64_ARM, "ARM64_ARM");
			parser.Add(CpuId.ARM64_X86, "ARM64_X86");
		}

		public static CpuId Parse(string value)
		{
			return parser.Parse(value.ToUpper(CultureInfo.InvariantCulture));
		}
	}
}
